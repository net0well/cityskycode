using System.Collections.Generic;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Simulation
{
    [TestFixture]
    public sealed class SimulationTests
    {
        private sealed class RecordingCommand : ICityCommand
        {
            private readonly List<int> _log;
            private readonly int _id;

            public RecordingCommand(List<int> log, int id)
            {
                _log = log;
                _id = id;
            }

            public CommandResult Apply(CityState state, in SimulationContext context)
            {
                _log.Add(_id);
                return CommandResult.Ok;
            }
        }

        private static CitySky.Domain.Simulation NewSim() =>
            new CitySky.Domain.Simulation(worldSeed: 1UL, systems: new ISimulationSystem[] { new HeartbeatSystem(), new CalendarSystem() });

        [Test]
        public void Heartbeat_Increments_Each_Tick()
        {
            var sim = NewSim();
            sim.RunTicks(10);
            Assert.That(sim.State.Heartbeat, Is.EqualTo(10L));
            Assert.That(sim.CurrentTick, Is.EqualTo(10L));
        }

        [Test]
        public void Calendar_Rolls_Over_At_Midnight_And_Emits_Event()
        {
            var sim = NewSim();
            var days = new List<long>();
            sim.Events.Subscribe(e =>
            {
                if (e is DayStartedEvent started)
                {
                    days.Add(started.DayOfGame);
                }
            });

            sim.RunTicks(SimulationClock.TicksPerDay + 1); // cruza a meia-noite uma vez
            Assert.That(sim.State.DayCounter, Is.EqualTo(1L));
            Assert.That(days, Is.EqualTo(new[] { 2L }));
        }

        [Test]
        public void Commands_Apply_In_Enqueue_Order_At_Same_Tick()
        {
            var sim = NewSim();
            var log = new List<int>();
            sim.Commands.Enqueue(new RecordingCommand(log, 1), targetTick: 0);
            sim.Commands.Enqueue(new RecordingCommand(log, 2), targetTick: 0);
            sim.Commands.Enqueue(new RecordingCommand(log, 3), targetTick: 0);

            sim.RunTick();
            Assert.That(log, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Commands_Apply_Only_At_Target_Tick()
        {
            var sim = NewSim();
            var log = new List<int>();
            sim.Commands.Enqueue(new RecordingCommand(log, 42), targetTick: 3);

            sim.RunTicks(3); // ticks 0,1,2
            Assert.That(log, Is.Empty);

            sim.RunTick(); // tick 3
            Assert.That(log, Is.EqualTo(new[] { 42 }));
        }
    }
}
