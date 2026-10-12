using CitySky.CodeAnalysis;
using CitySky.Contracts;
using CitySky.Runtime;
using NUnit.Framework;

namespace CitySky.CodeHost.Tests
{
    // F1-70..73: Host ↔ Worker como processos separados (Worker real iniciado pelo ISandboxLauncher).
    [TestFixture]
    public sealed class RunCoordinatorTests
    {
        private WorkerPool _pool = null!;
        private RunCoordinator _coordinator = null!;

        private static Budgets Budgets(int timeMs = 5000, long ticks = 5_000_000, int depth = 256, int memMb = 256) =>
            new() { TimeMs = timeMs, BudgetTicks = ticks, RecursionDepth = depth, MemoryMegabytes = memMb };

        [OneTimeSetUp]
        public void Setup()
        {
            var pipeline = new Pipeline(ReferenceSet.Create());
            _pool = new WorkerPool(new ProcessSandboxLauncher());
            _coordinator = new RunCoordinator(pipeline, _pool);
        }

        [OneTimeTearDown]
        public void TearDown() => _pool.Dispose();

        [Test]
        public void Valid_Program_Runs_In_Worker_And_Prints()
        {
            RunOutcome outcome = _coordinator.Run(
                "var city = CitySky.CityGame.Current; city.Log(\"Bom dia, cidade!\");", Budgets());
            Assert.That(outcome.Compiled, Is.True, string.Join(" | ", outcome.Diagnostics));
            Assert.That(outcome.Execution!.Status, Is.EqualTo(SandboxStatus.Ok), outcome.Execution.Message);
            Assert.That(outcome.Execution.Console, Does.Contain("Bom dia, cidade!"));
        }

        [Test]
        public void Compile_Failure_Reported_Without_Running()
        {
            RunOutcome outcome = _coordinator.Run("System.IO.File.ReadAllText(\"x\");", Budgets());
            Assert.That(outcome.Compiled, Is.False);
            Assert.That(outcome.Stage, Is.EqualTo("L1"));
            Assert.That(outcome.Execution, Is.Null);
        }

        [Test]
        public void Infinite_Loop_Hits_Budget_In_Worker()
        {
            RunOutcome outcome = _coordinator.Run("int x = 0; while (true) { x++; }", Budgets(ticks: 10_000));
            Assert.That(outcome.Compiled, Is.True);
            Assert.That(outcome.Execution!.Status, Is.EqualTo(SandboxStatus.Budget));
        }

        [Test]
        public void Pool_Reuses_And_Recovers_Across_Runs()
        {
            // Uma execução que estoura o orçamento, depois uma válida: o pool continua utilizável.
            _coordinator.Run("int x = 0; while (true) { x++; }", Budgets(ticks: 5_000));
            RunOutcome after = _coordinator.Run(
                "var city = CitySky.CityGame.Current; city.Log(\"ainda vivo\");", Budgets());
            Assert.That(after.Execution!.Status, Is.EqualTo(SandboxStatus.Ok));
            Assert.That(after.Execution.Console, Does.Contain("ainda vivo"));
        }
    }
}
