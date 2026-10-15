namespace CitySky.Contracts
{
    /// <summary>Cancelar uma execução em andamento.</summary>
    public sealed class Cancel
    {
        public string RunId { get; init; } = string.Empty;
    }
}
