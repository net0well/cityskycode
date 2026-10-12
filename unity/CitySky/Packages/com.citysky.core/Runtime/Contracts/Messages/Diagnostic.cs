namespace CitySky.Contracts
{
    /// <summary>Diagnóstico de compilação (código CS/CSK, posição, mensagem pt-BR).</summary>
    public sealed class Diagnostic
    {
        public string Id { get; init; } = string.Empty;
        public DiagnosticSeverity Severity { get; init; }
        public string Message { get; init; } = string.Empty;
        public int StartLine { get; init; }
        public int StartColumn { get; init; }
        public int EndLine { get; init; }
        public int EndColumn { get; init; }
    }
}
