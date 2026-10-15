using CitySky.Contracts;
using CitySky.Infrastructure;
using NUnit.Framework;

namespace CitySky.EditModeTests
{
    // F1-74: o serializer da Unity (Newtonsoft) deve produzir o MESMO JSON que o Code Host (.NET).
    // O golden é o mesmo de CitySky.Contracts.Tests — garante compatibilidade entre as pontas.
    public sealed class ProtocolJsonTests
    {
        [Test]
        public void Hello_Envelope_Matches_Dotnet_Golden()
        {
            var envelope = new Envelope<Hello>
            {
                Id = "abc",
                Type = MessageType.Hello,
                Payload = new Hello { ProtocolVersion = 1, GameVersion = "0.1.0" },
            };

            string json = ProtocolJson.Serialize(envelope);
            Assert.That(json, Is.EqualTo(
                "{\"v\":1,\"id\":\"abc\",\"type\":\"Hello\",\"payload\":{\"protocolVersion\":1,\"gameVersion\":\"0.1.0\"}}"));
        }

        [Test]
        public void CompileAndRun_Roundtrips()
        {
            var envelope = new Envelope<CompileAndRun>
            {
                Id = "c1",
                Type = MessageType.CompileAndRun,
                Payload = new CompileAndRun
                {
                    RunId = "run-1",
                    Mode = ExecutionMode.Script,
                    Source = "city.Log(\"Olá\");",
                    Budgets = new Budgets { TimeMs = 2000, BudgetTicks = 5_000_000, RecursionDepth = 256, MemoryMegabytes = 256 },
                },
            };

            Envelope<CompileAndRun> copy = ProtocolJson.Deserialize<CompileAndRun>(ProtocolJson.Serialize(envelope));
            Assert.That(copy.Type, Is.EqualTo(MessageType.CompileAndRun));
            Assert.That(copy.Payload.RunId, Is.EqualTo("run-1"));
            Assert.That(copy.Payload.Source, Is.EqualTo("city.Log(\"Olá\");"));
            Assert.That(copy.Payload.Budgets.BudgetTicks, Is.EqualTo(5_000_000L));
        }
    }
}
