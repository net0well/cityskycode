// F1-21. Shim para habilitar propriedades `init` em C# 9 (ausente em netstandard2.1).
// Compila na Unity (Mono/IL2CPP) e no .NET 10. Mantido internal para não vazar na API pública.

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
