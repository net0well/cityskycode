using System.Collections.Generic;
using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Simulation
{
    // F1-110 (lado .NET): mesma seed + mesmos comandos → mesmo hash. O mesmo teste roda em Unity
    // EditMode (Mono) contra o mesmo golden para fechar o determinismo entre runtimes.
    [TestFixture]
    public sealed class DeterminismTests
    {
        private const ulong Seed = 0xC175C0DE5EEDUL;
        private const long Ticks = SimulationClock.TicksPerDay + 137;

        // Golden capturado desta implementação; trava o determinismo daqui para a frente
        // (o mesmo cenário em Unity EditMode/Mono deve bater com este valor — F1-110).
        private const ulong GoldenHash = 15550467706480073983UL;

        private static ulong RunScenario(ulong seed)
        {
            var sim = new HeadlessSimulation(seed);
            sim.RunTicks(Ticks);
            return sim.Hash();
        }

        [Test]
        public void Two_Runs_Same_Seed_Produce_Same_Hash()
        {
            ulong first = RunScenario(Seed);
            ulong second = RunScenario(Seed);
            TestContext.Out.WriteLine($"hash=0x{first:X16}");
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Different_Seed_Produces_Different_Hash()
        {
            Assert.That(RunScenario(Seed), Is.Not.EqualTo(RunScenario(Seed ^ 0x1UL)));
        }

        [Test]
        public void Hash_Matches_Golden()
        {
            Assert.That(RunScenario(Seed), Is.EqualTo(GoldenHash));
        }
    }
}
