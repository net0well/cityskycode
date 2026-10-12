using System.Collections.Generic;
using CitySky.CodeAnalysis;
using CitySky.Runtime;

namespace CitySky.CodeHost;

/// <summary>Resultado de ponta a ponta: compilação (+diagnósticos) e, se compilou, execução no Worker.</summary>
public sealed record RunOutcome(
    bool Compiled,
    string Stage,
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<string> Diagnostics,
    SandboxResult? Execution);
