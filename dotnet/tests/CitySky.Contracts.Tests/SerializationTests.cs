using CitySky.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CitySky.Contracts.Tests
{
    [TestFixture]
    public sealed class SerializationTests
    {
        // Configuração canônica do protocolo (TypeNameHandling.None — F1-61). As duas pontas
        // (Code Host e Unity/Infrastructure) devem usar exatamente estas opções.
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
        };

        private static string Serialize<T>(T value) => JsonConvert.SerializeObject(value, Settings);

        private static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings)!;

        [Test]
        public void Hello_Envelope_Golden()
        {
            var envelope = new Envelope<Hello>
            {
                Id = "abc",
                Type = MessageType.Hello,
                Payload = new Hello { ProtocolVersion = 1, GameVersion = "0.1.0" },
            };

            string json = Serialize(envelope);
            Assert.That(json, Is.EqualTo(
                "{\"v\":1,\"id\":\"abc\",\"type\":\"Hello\",\"payload\":{\"protocolVersion\":1,\"gameVersion\":\"0.1.0\"}}"));
        }

        [Test]
        public void No_Type_Handling_Metadata_Is_Emitted()
        {
            string json = Serialize(new Envelope<Cancel>
            {
                Id = "r1",
                Type = MessageType.Cancel,
                Payload = new Cancel { RunId = "run-7" },
            });
            Assert.That(json, Does.Not.Contain("$type"));
        }

        [Test]
        public void CompileAndRun_Roundtrips()
        {
            var original = new Envelope<CompileAndRun>
            {
                Id = "c1",
                Type = MessageType.CompileAndRun,
                Payload = new CompileAndRun
                {
                    RunId = "run-1",
                    Mode = ExecutionMode.Script,
                    EntryPoint = "Program",
                    Source = "city.Log(\"Olá\");",
                    Budgets = new Budgets { TimeMs = 2000, BudgetTicks = 5_000_000, RecursionDepth = 256, MemoryMegabytes = 256 },
                },
            };

            var copy = Deserialize<Envelope<CompileAndRun>>(Serialize(original));
            Assert.That(copy.V, Is.EqualTo(1));
            Assert.That(copy.Type, Is.EqualTo(MessageType.CompileAndRun));
            Assert.That(copy.Payload!.RunId, Is.EqualTo("run-1"));
            Assert.That(copy.Payload.Source, Is.EqualTo("city.Log(\"Olá\");"));
            Assert.That(copy.Payload.Budgets.BudgetTicks, Is.EqualTo(5_000_000L));
        }

        [Test]
        public void RunResult_Omits_Null_Error()
        {
            string json = Serialize(new Envelope<RunResult>
            {
                Id = "r2",
                Type = MessageType.RunResult,
                Payload = new RunResult { RunId = "run-1", Success = true, Console = "Olá" },
            });
            Assert.That(json, Does.Not.Contain("error"));
        }

        [Test]
        public void Diagnostics_Roundtrips()
        {
            var original = new Envelope<Diagnostics>
            {
                Id = "d1",
                Type = MessageType.Diagnostics,
                Payload = new Diagnostics
                {
                    Items = new[]
                    {
                        new Diagnostic { Id = "CS1002", Severity = DiagnosticSeverity.Error, Message = "; esperado", StartLine = 3, StartColumn = 10, EndLine = 3, EndColumn = 11 },
                    },
                },
            };

            var copy = Deserialize<Envelope<Diagnostics>>(Serialize(original));
            Assert.That(copy.Payload!.Items, Has.Count.EqualTo(1));
            Assert.That(copy.Payload.Items[0].Id, Is.EqualTo("CS1002"));
            Assert.That(copy.Payload.Items[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
        }
    }
}
