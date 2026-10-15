namespace CitySky.Contracts
{
    /// <summary>Métricas de uma execução.</summary>
    public sealed class RunMetrics
    {
        public int CompileMs { get; init; }
        public int RunMs { get; init; }
        public int CommandCount { get; init; }
    }
}
