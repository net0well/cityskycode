// Shim para `init` em C# 9 (netstandard2.1). Cada assembly compartilhado tem o seu (internal),
// pois Contracts não referencia Domain. Ver Domain/Internal/IsExternalInit.cs (F1-21).

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
