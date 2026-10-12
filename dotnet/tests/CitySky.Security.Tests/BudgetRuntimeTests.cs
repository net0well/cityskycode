using CitySky.CodeAnalysis;
using CitySky.Runtime;
using NUnit.Framework;

namespace CitySky.Security.Tests
{
    // F1-75 — orçamentos em tempo de execução (L3): laço, recursão, memória, e o estouro que o
    // jogador não consegue capturar. Executa o assembly emitido num ALC coletável (SandboxExecutor).
    [TestFixture]
    public sealed class BudgetRuntimeTests
    {
        private static Pipeline _pipeline = null!;

        [OneTimeSetUp]
        public void Setup() => _pipeline = new Pipeline(ReferenceSet.Create());

        private SandboxResult CompileAndRun(string source, SandboxBudgets budgets)
        {
            CompileResult compiled = _pipeline.Compile(source);
            Assert.That(compiled.Ok, Is.True, "compilação deveria passar: " + string.Join(" | ", compiled.Diagnostics));
            return SandboxExecutor.Run(compiled.Assembly!, compiled.Pdb!, budgets);
        }

        [Test]
        public void Valid_Program_Prints_To_Console()
        {
            SandboxResult r = CompileAndRun(
                "var city = CitySky.CityGame.Current; city.Log(\"Bom dia, cidade!\");",
                new SandboxBudgets(BudgetTicks: 5_000_000, MaxDepth: 256, AllocationLimitBytes: 256L * 1024 * 1024, OutputLimitChars: 64 * 1024));
            Assert.That(r.Status, Is.EqualTo(SandboxStatus.Ok), r.Message);
            Assert.That(r.Console, Does.Contain("Bom dia, cidade!"));
        }

        [Test]
        public void Infinite_Loop_Hits_Budget()
        {
            SandboxResult r = CompileAndRun(
                "int x = 0; while (true) { x++; }",
                new SandboxBudgets(BudgetTicks: 10_000, MaxDepth: 256, AllocationLimitBytes: 256L * 1024 * 1024, OutputLimitChars: 1024));
            Assert.That(r.Status, Is.EqualTo(SandboxStatus.Budget));
            Assert.That(r.Reason, Is.EqualTo(nameof(BudgetReason.Loop)));
            Assert.That(r.Line, Is.GreaterThan(0));
        }

        [Test]
        public void Infinite_Recursion_Hits_Budget_Before_Stack_Overflow()
        {
            SandboxResult r = CompileAndRun(
                "void Recurse() { Recurse(); }\nRecurse();",
                new SandboxBudgets(BudgetTicks: 5_000_000, MaxDepth: 64, AllocationLimitBytes: 256L * 1024 * 1024, OutputLimitChars: 1024));
            Assert.That(r.Status, Is.EqualTo(SandboxStatus.Budget));
            Assert.That(r.Reason, Is.EqualTo(nameof(BudgetReason.Recursion)));
        }

        [Test]
        public void Runaway_Allocation_Hits_Budget()
        {
            SandboxResult r = CompileAndRun(
                "var list = new System.Collections.Generic.List<int>(); while (true) { list.Add(1); }",
                new SandboxBudgets(BudgetTicks: 500_000_000, MaxDepth: 256, AllocationLimitBytes: 1L * 1024 * 1024, OutputLimitChars: 1024));
            Assert.That(r.Status, Is.EqualTo(SandboxStatus.Budget));
        }

        [Test]
        public void Student_Cannot_Swallow_Budget_Exception()
        {
            // O catch do jogador recebe o filtro when(!__Budget.Exceeded): o estouro escapa mesmo assim.
            SandboxResult r = CompileAndRun(
                "try { int x = 0; while (true) { x++; } } catch { System.Console.WriteLine(\"peguei\"); }",
                new SandboxBudgets(BudgetTicks: 10_000, MaxDepth: 256, AllocationLimitBytes: 256L * 1024 * 1024, OutputLimitChars: 1024));
            Assert.That(r.Status, Is.EqualTo(SandboxStatus.Budget));
            Assert.That(r.Console, Does.Not.Contain("peguei"));
        }
    }
}
