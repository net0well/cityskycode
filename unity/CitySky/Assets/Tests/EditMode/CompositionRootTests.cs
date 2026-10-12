using CitySky.Application;
using CitySky.Bootstrap;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.EditModeTests
{
    // F1-43: a raiz de composição monta o grafo e avança a simulação; eventos de domínio viram log.
    public sealed class CompositionRootTests
    {
        [Test]
        public void Advances_Simulation_And_Logs_Day_Change()
        {
            var log = new CapturingLog();
            var root = new CompositionRoot(1UL, log);

            // Avança um dia de calendário (11 520 ticks) em 10x; cada frame de 0,1 s roda 4 ticks.
            int safety = 0;
            while (root.Runner.CurrentTick <= SimulationClock.TicksPerDay && safety++ < 100_000)
            {
                root.Advance(0.1, GameSpeed.Fastest, UnlimitedSimulationBudget.Instance);
            }

            Assert.That(root.Runner.CurrentTick, Is.GreaterThan(SimulationClock.TicksPerDay));
            Assert.That(root.Summary.DayOfGame, Is.EqualTo(2));
            Assert.That(log.Messages, Has.Some.Contains("Novo dia: 2"));
        }

        [Test]
        public void Read_Model_Tracks_Clock()
        {
            var root = new CompositionRoot(1UL, new CapturingLog());
            root.Advance(0.1, GameSpeed.Fastest, UnlimitedSimulationBudget.Instance); // 4 ticks
            Assert.That(root.Summary.DayOfGame, Is.EqualTo(1));
            Assert.That(root.Summary.Speed, Is.EqualTo(GameSpeed.Fastest));
        }
    }
}
