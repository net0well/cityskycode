using System;
using System.Collections.Generic;

namespace CitySky.CodeAnalysis;

/// <summary>
/// Lista de APIs que o código do jogador pode usar (L1, CODE_EXECUTION_SECURITY.md). "Negar por padrão":
/// só o que está aqui passa. v0 (F1-71): o necessário para <c>city.Log</c>, literais e aritmética básica.
/// Ampliar exige security-auditor + caso no corpus (regra inviolável). No produto vira allowlist.json.
/// </summary>
public static class Allowlist
{
    /// <summary>Tipos cujos membros (todos) são permitidos.</summary>
    public static readonly HashSet<string> AllTypeMembers = new(StringComparer.Ordinal)
    {
        "System.Object",
        "System.String",
        "System.Char",
        "System.Boolean",
        "System.Byte", "System.SByte",
        "System.Int16", "System.UInt16",
        "System.Int32", "System.UInt32",
        "System.Int64", "System.UInt64",
        "System.Single", "System.Double", "System.Decimal",
        "System.Math",
        "System.TimeSpan",
        "System.Text.StringBuilder",
        "System.Collections.Generic.List`1",
        "System.Collections.Generic.Dictionary`2",
        "System.Collections.Generic.HashSet`1",
        "System.Collections.Generic.KeyValuePair`2",
        "System.Collections.Generic.IEnumerable`1",
        "System.Collections.Generic.IReadOnlyList`1",
        "System.Collections.Generic.IList`1",
        // Player API v0.
        "CitySky.CityGame",
        "CitySky.City",
    };

    /// <summary>Tipos com só alguns membros liberados.</summary>
    public static readonly Dictionary<string, HashSet<string>> SomeMembers = new(StringComparer.Ordinal)
    {
        // Console vira o console do jogo (com limite de saída); só escrita.
        ["System.Console"] = new(StringComparer.Ordinal) { "WriteLine", "Write" },
    };

    /// <summary>Tipos necessários como tipo/base/genérico, sem membros chamáveis diretos.</summary>
    public static readonly HashSet<string> AllowedTypesNoMembers = new(StringComparer.Ordinal)
    {
        "System.Linq.Enumerable",
        "System.Void",
        "System.Array",
        "System.ValueType",
        "System.Enum",
        "System.Exception",
        "System.IDisposable",
        "System.Nullable`1",
        "System.Func`1", "System.Func`2", "System.Func`3",
        "System.Action", "System.Action`1", "System.Action`2",
    };

    /// <summary>Namespaces inteiros proibidos (reforço rápido; a checagem por símbolo é a que vale).</summary>
    public static readonly string[] BannedNamespacePrefixes =
    {
        "System.IO",
        "System.Net",
        "System.Diagnostics",
        "System.Reflection",
        "System.Runtime.InteropServices",
        "System.Threading",
        "System.Security",
        "System.Linq.Expressions",
        "Microsoft.Win32",
        "System.Runtime.Loader",
    };

    /// <summary>O membro, na forma canônica, é permitido?</summary>
    public static bool IsMemberAllowed(string containingType, string memberName)
    {
        if (AllTypeMembers.Contains(containingType))
        {
            return true;
        }

        return SomeMembers.TryGetValue(containingType, out HashSet<string>? members) && members.Contains(memberName);
    }

    /// <summary>O tipo pode ser referenciado (como tipo de variável, base, genérico)?</summary>
    public static bool IsTypeAllowed(string typeName) =>
        AllTypeMembers.Contains(typeName)
        || SomeMembers.ContainsKey(typeName)
        || AllowedTypesNoMembers.Contains(typeName);
}
