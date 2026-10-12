using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Application.Tests.ReadModels
{
    [TestFixture]
    public sealed class CitySummaryTests
    {
        [Test]
        public void Reports_Dirty_Only_When_Something_Changes()
        {
            var summary = new CitySummary();

            bool first = summary.Update(new SimulationClock(0), GameSpeed.Normal, speedLimited: false);
            Assert.That(first, Is.True);
            Assert.That(summary.Version, Is.EqualTo(1));

            // Mesmo minuto (tick < 8) e mesma velocidade → sem mudança visível.
            bool same = summary.Update(new SimulationClock(1), GameSpeed.Normal, speedLimited: false);
            Assert.That(same, Is.False);
            Assert.That(summary.Version, Is.EqualTo(1));

            // Vira o minuto (8 ticks) → muda.
            bool minuteChanged = summary.Update(new SimulationClock(SimulationClock.TicksPerMinute), GameSpeed.Normal, false);
            Assert.That(minuteChanged, Is.True);
            Assert.That(summary.Version, Is.EqualTo(2));
            Assert.That(summary.MinuteOfHour, Is.EqualTo(1));

            // Troca de velocidade → muda.
            bool speedChanged = summary.Update(new SimulationClock(SimulationClock.TicksPerMinute), GameSpeed.Fastest, false);
            Assert.That(speedChanged, Is.True);
            Assert.That(summary.Speed, Is.EqualTo(GameSpeed.Fastest));
        }
    }
}
