using System;
using System.Collections.Generic;
using CitySky.CodeAnalysis;
using CitySky.Contracts;
using CitySky.Runtime;

namespace CitySky.CodeHost;

/// <summary>
/// Orquestra uma execução: Pipeline (compila/analisa) → WorkerPool (executa no processo isolado).
/// É o núcleo testável do Code Host, independente do transporte com a Unity (F1-70..73).
/// </summary>
public sealed class RunCoordinator
{
    private const int DefaultOutputLimitChars = 64 * 1024;

    private readonly Pipeline _pipeline;
    private readonly WorkerPool _pool;

    public RunCoordinator(Pipeline pipeline, WorkerPool pool)
    {
        _pipeline = pipeline;
        _pool = pool;
    }

    /// <summary>Diagnósticos ao digitar (compila sem executar). F1-93.</summary>
    public IReadOnlyList<Diagnostic> Diagnose(string source) => _pipeline.Diagnose(source);

    public RunOutcome Run(string source, Budgets budgets)
    {
        ArgumentNullException.ThrowIfNull(budgets);

        CompileResult compiled = _pipeline.Compile(source);
        if (!compiled.Ok)
        {
            return new RunOutcome(false, compiled.Stage, compiled.Findings, compiled.Diagnostics, null);
        }

        var sandboxBudgets = new SandboxBudgets(
            BudgetTicks: budgets.BudgetTicks,
            MaxDepth: budgets.RecursionDepth,
            AllocationLimitBytes: (long)budgets.MemoryMegabytes * 1024 * 1024,
            OutputLimitChars: DefaultOutputLimitChars);

        SandboxResult result = _pool.Execute(
            new WorkerRequest(compiled.Assembly!, compiled.Pdb!, sandboxBudgets),
            wallTimeoutMs: budgets.TimeMs);

        return new RunOutcome(true, compiled.Stage, compiled.Findings, compiled.Diagnostics, result);
    }
}
