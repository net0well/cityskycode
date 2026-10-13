using System.Collections.Generic;
using CitySky.Contracts;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Abstração do Code Host vista pela Unity (Dependency Inversion): executar código e obter
    /// diagnósticos. Implementada pelo <see cref="CodeHostClient"/> (IPC real) e por dublês nos testes.
    /// </summary>
    public interface ICodeHost
    {
        RunResult Run(string source);

        IReadOnlyList<Diagnostic> GetDiagnostics(string source);
    }
}
