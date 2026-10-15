using System.Collections.Generic;

namespace CitySky.CodeAnalysis;

/// <summary>Resultado do pipeline: estágio alcançado, achados/diagnósticos e, se emitido, o binário.</summary>
public sealed record CompileResult(
    bool Ok,
    string Stage,                         // syntax | L1 | compile | L2 | emitted
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<string> Diagnostics,
    byte[]? Assembly,
    byte[]? Pdb,
    StageTimings Timings);
