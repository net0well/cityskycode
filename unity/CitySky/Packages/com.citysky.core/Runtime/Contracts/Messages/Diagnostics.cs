using System;
using System.Collections.Generic;

namespace CitySky.Contracts
{
    /// <summary>Conjunto de diagnósticos (sublinhados ao digitar).</summary>
    public sealed class Diagnostics
    {
        public IReadOnlyList<Diagnostic> Items { get; init; } = Array.Empty<Diagnostic>();
    }
}
