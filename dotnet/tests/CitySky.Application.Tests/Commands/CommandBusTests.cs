using System;
using CitySky.Application;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Application.Tests.Commands
{
    [TestFixture]
    public sealed class CommandBusTests
    {
        private static Simulation NewSim() =>
            new Simulation(1UL, Array.Empty<ISimulationSystem>());

        [Test]
        public void Valid_Command_Is_Enqueued_And_Applied_At_Target_Tick()
        {
            Simulation sim = NewSim();
            var bus = new CommandBus(sim);
            var command = new MarkerCommand();

            CommandResult result = bus.Submit(command, targetTick: 2);
            Assert.That(result.Success, Is.True);

            sim.RunTicks(2); // ticks 0 e 1
            Assert.That(command.Applied, Is.False);

            sim.RunTick(); // tick 2
            Assert.That(command.Applied, Is.True);
            Assert.That(command.AppliedAtTick, Is.EqualTo(2L));
        }

        [Test]
        public void Rejected_Command_Is_Not_Enqueued()
        {
            Simulation sim = NewSim();
            var bus = new CommandBus(sim, new RejectingValidator());
            var command = new MarkerCommand();

            CommandResult result = bus.Submit(command, targetTick: 0);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.EqualTo(CommandError.Rejected));
            Assert.That(sim.Commands.Count, Is.EqualTo(0));

            sim.RunTick();
            Assert.That(command.Applied, Is.False);
        }
    }
}
