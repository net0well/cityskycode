namespace CitySky.Contracts
{
    /// <summary>Orçamentos de execução (CODE_EXECUTION §7).</summary>
    public sealed class Budgets
    {
        public int TimeMs { get; init; }
        public long BudgetTicks { get; init; }
        public int RecursionDepth { get; init; }
        public int MemoryMegabytes { get; init; }
    }
}
