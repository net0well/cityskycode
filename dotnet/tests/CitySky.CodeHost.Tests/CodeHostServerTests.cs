using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using CitySky.CodeAnalysis;
using CitySky.Contracts;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CitySky.CodeHost.Tests
{
    // F1-70: despacho do protocolo (Hello, CompileAndRun) e round-trip real por named pipe.
    [TestFixture]
    public sealed class CodeHostServerTests
    {
        private WorkerPool _pool = null!;
        private CodeHostServer _server = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var pipeline = new Pipeline(ReferenceSet.Create());
            _pool = new WorkerPool(new ProcessSandboxLauncher());
            _server = new CodeHostServer(new RunCoordinator(pipeline, _pool));
        }

        [OneTimeTearDown]
        public void TearDown() => _pool.Dispose();

        private static byte[] Hello() => ProtocolSerializer.Serialize(new Envelope<Hello>
        {
            Id = "h1",
            Type = MessageType.Hello,
            Payload = new Hello { ProtocolVersion = Protocol.Version, GameVersion = "test" },
        });

        private static byte[] Run(string source) => ProtocolSerializer.Serialize(new Envelope<CompileAndRun>
        {
            Id = "r1",
            Type = MessageType.CompileAndRun,
            Payload = new CompileAndRun
            {
                RunId = "run-1",
                Mode = ExecutionMode.Script,
                Source = source,
                Budgets = new Budgets { TimeMs = 5000, BudgetTicks = 5_000_000, RecursionDepth = 256, MemoryMegabytes = 256 },
            },
        });

        [Test]
        public void Dispatch_Hello_Replies_Hello()
        {
            byte[] reply = _server.Dispatch(Hello())!;
            (int _, string _, string type, JObject? payload) = ProtocolSerializer.ReadEnvelope(reply);
            Assert.That(type, Is.EqualTo(MessageType.Hello));
            Hello hello = ProtocolSerializer.ReadPayload<Hello>(payload)!;
            Assert.That(hello.ProtocolVersion, Is.EqualTo(Protocol.Version));
            Assert.That(hello.GameVersion, Is.Not.Empty);
        }

        [Test]
        public void Dispatch_CompileAndRun_Returns_RunResult()
        {
            byte[] reply = _server.Dispatch(Run("var city = CitySky.CityGame.Current; city.Log(\"oi\");"))!;
            (int _, string _, string type, JObject? payload) = ProtocolSerializer.ReadEnvelope(reply);
            Assert.That(type, Is.EqualTo(MessageType.RunResult));
            RunResult result = ProtocolSerializer.ReadPayload<RunResult>(payload)!;
            Assert.That(result.Success, Is.True, result.Error);
            Assert.That(result.Console, Does.Contain("oi"));
        }

        [Test]
        public void Dispatch_CompileAndRun_Reports_Compile_Failure()
        {
            byte[] reply = _server.Dispatch(Run("System.IO.File.ReadAllText(\"x\");"))!;
            RunResult result = ProtocolSerializer.ReadPayload<RunResult>(ProtocolSerializer.ReadEnvelope(reply).Payload)!;
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.Not.Null);
        }

        [Test]
        public void Named_Pipe_RoundTrip_Hello()
        {
            string name = "citysky-test-" + Guid.NewGuid().ToString("N");
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            using var client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);

            Task waitConnect = server.WaitForConnectionAsync();
            client.Connect(5000);
            Assert.That(waitConnect.Wait(5000), Is.True);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            Task serve = Task.Run(() => _server.ServeAsync(server, cts.Token));

            MessageFraming.WriteFrame(client, Hello());
            Assert.That(MessageFraming.TryReadFrame(client, out byte[] reply), Is.True);

            (int _, string _, string type, JObject? payload) = ProtocolSerializer.ReadEnvelope(reply);
            Assert.That(type, Is.EqualTo(MessageType.Hello));
            Assert.That(ProtocolSerializer.ReadPayload<Hello>(payload)!.GameVersion, Is.Not.Empty);

            cts.Cancel();
            client.Close();
        }
    }
}
