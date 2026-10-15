using CitySky.CodeAnalysis;
using CitySky.Contracts;
using CitySky.Runtime;
using NUnit.Framework;

namespace CitySky.CodeHost.Tests
{
    // F1-51: o Worker roda dentro de um Job Object (limites + KILL_ON_JOB_CLOSE). Windows-only;
    // o canário Linux usa o ProcessSandboxLauncher, então este fixture é pulado lá.
    [TestFixture]
    [Platform(Include = "Win")]
    public sealed class JobObjectLauncherTests
    {
        private WorkerPool _pool = null!;
        private RunCoordinator _coordinator = null!;

        private static Budgets Budgets(int timeMs = 5000, long ticks = 5_000_000, int depth = 256, int memMb = 256) =>
            new() { TimeMs = timeMs, BudgetTicks = ticks, RecursionDepth = depth, MemoryMegabytes = memMb };

        [OneTimeSetUp]
        public void Setup()
        {
            var pipeline = new Pipeline(ReferenceSet.Create());
            _pool = new WorkerPool(new JobObjectSandboxLauncher());
            _coordinator = new RunCoordinator(pipeline, _pool);
        }

        [OneTimeTearDown]
        public void TearDown() => _pool.Dispose();

        [Test]
        public void Valid_Program_Runs_Inside_Job_And_Prints()
        {
            RunOutcome outcome = _coordinator.Run(
                "var city = CitySky.CityGame.Current; city.Log(\"dentro do job\");", Budgets());
            Assert.That(outcome.Compiled, Is.True);
            Assert.That(outcome.Execution!.Status, Is.EqualTo(SandboxStatus.Ok), outcome.Execution.Message);
            Assert.That(outcome.Execution.Console, Does.Contain("dentro do job"));
        }

        [Test]
        public void Infinite_Loop_Hits_Budget_Inside_Job()
        {
            RunOutcome outcome = _coordinator.Run("int x = 0; while (true) { x++; }", Budgets(ticks: 10_000));
            Assert.That(outcome.Execution!.Status, Is.EqualTo(SandboxStatus.Budget));
        }

        [Test]
        public void Pool_Recovers_After_Recycle()
        {
            _coordinator.Run("int x = 0; while (true) { x++; }", Budgets(ticks: 5_000));
            RunOutcome after = _coordinator.Run(
                "var city = CitySky.CityGame.Current; city.Log(\"vivo no job\");", Budgets());
            Assert.That(after.Execution!.Status, Is.EqualTo(SandboxStatus.Ok));
            Assert.That(after.Execution.Console, Does.Contain("vivo no job"));
        }
    }
}
