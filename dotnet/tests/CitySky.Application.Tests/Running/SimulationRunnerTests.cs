using System;
using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Application.Tests.Running
{
    [TestFixture]
    public sealed class SimulationRunnerTests
    {
        private static SimulationRunner NewRunner() =>
            new SimulationRunner(new Simulation(1UL, Array.Empty<ISimulationSystem>()));

        [Test]
        public void One_Second_At_Normal_Runs_Four_Ticks()
        {
            SimulationRunner runner = NewRunner();
            for (int i = 0; i < 10; i++) // 10 frames de 0,1 s = 1 s
            {
                runner.Advance(0.1, GameSpeed.Normal, UnlimitedSimulationBudget.Instance);
            }

            Assert.That(runner.CurrentTick, Is.EqualTo(4L));
        }

        [Test]
        public void Pause_Runs_No_Ticks()
        {
            SimulationRunner runner = NewRunner();
            runner.Advance(0.1, GameSpeed.Pause, UnlimitedSimulationBudget.Instance);
            Assert.That(runner.CurrentTick, Is.EqualTo(0L));
            Assert.That(runner.LastTicksRun, Is.EqualTo(0));
        }

        [Test]
        public void Budget_Defers_Ticks_Without_Skipping()
        {
            SimulationRunner runner = NewRunner();
            // Backlog de 4, mas o orçamento só deixa 2 por frame.
            runner.Advance(0.1, GameSpeed.Fastest, new CountingBudget(2));
            Assert.That(runner.LastTicksRun, Is.EqualTo(2));
            Assert.That(runner.CurrentTick, Is.EqualTo(2L));

            // Sem novo tempo, os 2 que sobraram rodam no frame seguinte — nenhum tick é pulado.
            runner.Advance(0.0, GameSpeed.Fastest, new CountingBudget(2));
            Assert.That(runner.CurrentTick, Is.EqualTo(4L));
        }

        [Test]
        public void Reports_Speed_Limited_On_Backlog_Clamp()
        {
            SimulationRunner runner = NewRunner();
            for (int i = 0; i < 20; i++)
            {
                runner.Advance(0.1, GameSpeed.Fastest, new CountingBudget(0)); // acumula sem rodar
            }

            Assert.That(runner.EffectiveSpeedLimited, Is.True);
        }
    }
}
