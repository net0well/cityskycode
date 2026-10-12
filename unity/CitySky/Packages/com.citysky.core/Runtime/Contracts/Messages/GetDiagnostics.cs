namespace CitySky.Contracts
{
    /// <summary>Pedido de diagnósticos ao digitar (sublinhados): compila sem executar e devolve os erros/avisos.</summary>
    public sealed class GetDiagnostics
    {
        public string Path { get; init; } = "Program.cs";
        public string Source { get; init; } = string.Empty;
    }
}
