using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.EditModeTests
{
    // F1-110 (lado Unity): o MESMO cenário e a MESMA seed do teste .NET (DeterminismTests) devem
    // produzir o MESMO hash em Unity Mono — prova de determinismo entre runtimes (.NET 10 ↔ Mono).
    // O golden é o capturado no .NET. Se divergir, o determinismo quebrou (R07).
    public sealed class DeterminismEditModeTests
    {
        private const ulong Seed = 0xC175C0DE5EEDUL;
        private const ulong GoldenHash = 15550467706480073983UL;

        [Test]
        public void Hash_Matches_Dotnet_Golden()
        {
            var sim = new HeadlessSimulation(Seed);
            sim.RunTicks(SimulationClock.TicksPerDay + 137);
            Assert.That(sim.Hash(), Is.EqualTo(GoldenHash));
        }

        [Test]
        public void Two_Runs_Same_Seed_Match()
        {
            var a = new HeadlessSimulation(Seed);
            var b = new HeadlessSimulation(Seed);
            a.RunTicks(5000);
            b.RunTicks(5000);
            Assert.That(a.Hash(), Is.EqualTo(b.Hash()));
        }
    }
}
