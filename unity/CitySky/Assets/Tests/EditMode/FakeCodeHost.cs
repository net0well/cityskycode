using System.Collections.Generic;
using CitySky.Contracts;
using CitySky.Infrastructure;

namespace CitySky.EditModeTests
{
    /// <summary>Dublê de <see cref="ICodeHost"/>: respostas fixas, sem processo nem IPC.</summary>
    internal sealed class FakeCodeHost : ICodeHost
    {
        public RunResult Run(string source) =>
            new RunResult { RunId = "fake", Success = true, Console = "saída fake" };

        public IReadOnlyList<Diagnostic> GetDiagnostics(string source) => new[]
        {
            new Diagnostic { Id = "CSK1001", Severity = DiagnosticSeverity.Error, Message = "proibido", StartLine = 1, StartColumn = 1, EndLine = 1, EndColumn = 2 },
        };
    }
}
