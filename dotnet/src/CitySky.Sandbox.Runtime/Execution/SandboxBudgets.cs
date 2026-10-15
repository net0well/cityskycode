namespace CitySky.Runtime;

/// <summary>Orçamentos de uma execução.</summary>
public sealed record SandboxBudgets(long BudgetTicks, int MaxDepth, long AllocationLimitBytes, int OutputLimitChars);
