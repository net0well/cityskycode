using System;
using System.Collections.Generic;

namespace CitySky.Contracts
{
    /// <summary>Resultado de uma execução: comandos emitidos, console, métricas e erro opcional.</summary>
    public sealed class RunResult
    {
        public string RunId { get; init; } = string.Empty;
        public bool Success { get; init; }
        public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();
        public string Console { get; init; } = string.Empty;
        public string? Error { get; init; }
        public RunMetrics Metrics { get; init; } = new RunMetrics();
    }
}
