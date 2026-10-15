using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Application.Tests.Running
{
    // F1-111: a velocidade não muda o resultado — mesmo estado após os mesmos ticks, em 1x e em 10x.
    [TestFixture]
    public sealed class SpeedIndependenceTests
    {
        private const long TwoDays = 2 * SimulationClock.TicksPerDay; // 23 040 ticks

        private static ulong RunToTick(GameSpeed speed, long targetTick)
        {
            var sim = new Simulation(0xC0FFEEUL, HeadlessSimulation.DefaultSystems());
            var runner = new SimulationRunner(sim);
            int safety = 0;
            while (runner.CurrentTick < targetTick && safety++ < 2_000_000)
            {
                runner.Advance(0.1, speed, UnlimitedSimulationBudget.Instance);
            }

            Assert.That(sim.CurrentTick, Is.EqualTo(targetTick), "deveria parar exatamente no tick alvo");
            return sim.State.ComputeHash();
        }

        [Test]
        public void Same_State_Hash_At_1x_And_10x()
        {
            ulong at1x = RunToTick(GameSpeed.Normal, TwoDays);
            ulong at10x = RunToTick(GameSpeed.Fastest, TwoDays);
            Assert.That(at10x, Is.EqualTo(at1x));
        }
    }
}
