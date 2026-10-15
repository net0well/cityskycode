namespace CitySky.Contracts
{
    /// <summary>Pedido de compilar e executar o código do jogador.</summary>
    public sealed class CompileAndRun
    {
        public string RunId { get; init; } = string.Empty;
        public ExecutionMode Mode { get; init; }
        public string EntryPoint { get; init; } = "Program";
        public string Source { get; init; } = string.Empty;
        public Budgets Budgets { get; init; } = new Budgets();
    }
}
