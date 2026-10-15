using System;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Time
{
    [TestFixture]
    public sealed class SimulationClockTests
    {
        [Test]
        public void Tick_Constants_Are_Consistent()
        {
            Assert.That(SimulationClock.TicksPerMinute * 60, Is.EqualTo(SimulationClock.TicksPerHour));
            Assert.That(SimulationClock.TicksPerHour * 24, Is.EqualTo(SimulationClock.TicksPerDay));
        }

        [Test]
        public void Hour_And_Day_Rollover()
        {
            Assert.That(new SimulationClock(0).DayOfGame, Is.EqualTo(1));
            Assert.That(new SimulationClock(SimulationClock.TicksPerHour).HourOfDay, Is.EqualTo(1));
            var midnight = new SimulationClock(SimulationClock.TicksPerDay);
            Assert.That(midnight.DayOfGame, Is.EqualTo(2));
            Assert.That(midnight.TickOfDay, Is.EqualTo(0));
            Assert.That(midnight.HourOfDay, Is.EqualTo(0));
        }

        [Test]
        public void Last_Tick_Of_Day()
        {
            var end = new SimulationClock(SimulationClock.TicksPerDay - 1);
            Assert.That(end.DayOfGame, Is.EqualTo(1));
            Assert.That(end.HourOfDay, Is.EqualTo(23));
            Assert.That(end.MinuteOfHour, Is.EqualTo(59));
        }

        [Test]
        public void Phases_By_Hour()
        {
            Assert.That(new SimulationClock(2 * SimulationClock.TicksPerHour).Phase, Is.EqualTo(DayPhase.Night));
            Assert.That(new SimulationClock(6 * SimulationClock.TicksPerHour).Phase, Is.EqualTo(DayPhase.Dawn));
            Assert.That(new SimulationClock(10 * SimulationClock.TicksPerHour).Phase, Is.EqualTo(DayPhase.Morning));
            Assert.That(new SimulationClock(14 * SimulationClock.TicksPerHour).Phase, Is.EqualTo(DayPhase.Afternoon));
            Assert.That(new SimulationClock(19 * SimulationClock.TicksPerHour).Phase, Is.EqualTo(DayPhase.Evening));
        }

        [Test]
        public void Hundred_Game_Years_Do_Not_Overflow()
        {
            long ticks = 100L * 365L * SimulationClock.TicksPerDay;
            var clock = new SimulationClock(ticks);
            Assert.That(clock.TmMilliseconds, Is.GreaterThan(0));
            Assert.That(clock.TcMilliseconds, Is.GreaterThan(0));
            Assert.That(clock.DayOfGame, Is.EqualTo(100L * 365L + 1L));
        }

        [Test]
        public void Negative_Tick_Rejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationClock(-1));
        }
    }
}
