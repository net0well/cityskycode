using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace CitySky.Spike.CodeHost;

/// <summary>
/// L2 do CODE_EXECUTION_SECURITY.md: confere o binário emitido contra a lista, pegando o que a análise
/// de símbolos (L1) poderia não ver (lowering, closures, interpoladores). Percorre TypeReference e
/// MemberReference pelo System.Reflection.Metadata e rejeita P/Invoke e módulos externos.
/// </summary>
public static class IlVerifier
{
    // No IL, além do que o aluno pode usar, aparecem tipos de suporte gerados pelo compilador.
    private static readonly string[] CompilerSupportPrefixes =
    {
        "System.Runtime.CompilerServices.",
        "System.Runtime.InteropServices.", // só atributos gerados; P/Invoke é barrado à parte
    };

    private static readonly HashSet<string> ExtraAllowedTypes = new(StringComparer.Ordinal)
    {
        "CitySky.Runtime.__Budget",
        "System.Runtime.CompilerServices.RuntimeHelpers",
        "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler",
        "System.Attribute",
        "System.Type", "System.RuntimeTypeHandle",
        // Atributos de infraestrutura emitidos pelo compilador no assembly/módulo (não são código do aluno;
        // não dão capacidade nenhuma). No produto, ficam numa seção "infra" da allowlist.json.
        "System.Diagnostics.DebuggableAttribute",
        "DebuggingModes",
        "System.Runtime.Versioning.TargetFrameworkAttribute",
    };

    private static readonly string[] AllowedAttributePrefixes =
    {
        "System.Reflection.Assembly", // AssemblyCompany/Configuration/FileVersion/Product/Title/Version...
    };

    public static IReadOnlyList<Finding> Verify(byte[] peBytes)
    {
        var findings = new List<Finding>();
        using var pe = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(peBytes));
        MetadataReader reader = pe.GetMetadataReader();

        if (reader.GetTableRowCount(TableIndex.ModuleRef) > 0)
        {
            findings.Add(new Finding("CSK1004", "Referência a módulo externo (P/Invoke) não é permitida.", 0));
        }

        foreach (MethodDefinitionHandle handle in reader.MethodDefinitions)
        {
            MethodDefinition method = reader.GetMethodDefinition(handle);
            if ((method.Attributes & MethodAttributes.PinvokeImpl) != 0)
            {
                findings.Add(new Finding("CSK1004", "Método com P/Invoke não é permitido.", 0));
            }
        }

        foreach (TypeReferenceHandle handle in reader.TypeReferences)
        {
            TypeReference type = reader.GetTypeReference(handle);
            string name = FullName(reader, type);
            if (!IsTypeAllowedAtIl(name))
            {
                findings.Add(new Finding("CSK1001", $"IL referencia o tipo proibido '{name}'.", 0));
            }
        }

        foreach (MemberReferenceHandle handle in reader.MemberReferences)
        {
            MemberReference member = reader.GetMemberReference(handle);
            if (member.Parent.Kind != HandleKind.TypeReference)
            {
                continue; // tipos genéricos instanciados aparecem na tabela de TypeReference e são checados lá
            }

            TypeReference parent = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
            string typeName = FullName(reader, parent);
            string memberName = reader.GetString(member.Name);
            if (member.GetKind() == MemberReferenceKind.Method
                && !Allowlist.IsMemberAllowed(typeName, memberName)
                && !IsTypeAllowedAtIl(typeName))
            {
                findings.Add(new Finding("CSK1001", $"IL chama '{typeName}.{memberName}', que não está disponível.", 0));
            }
        }

        return findings;
    }

    private static bool IsTypeAllowedAtIl(string typeName)
    {
        if (Allowlist.IsTypeAllowed(typeName) || ExtraAllowedTypes.Contains(typeName))
        {
            return true;
        }

        foreach (string prefix in CompilerSupportPrefixes)
        {
            if (typeName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (string prefix in AllowedAttributePrefixes)
        {
            if (typeName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string FullName(MetadataReader reader, TypeReference type)
    {
        string ns = type.Namespace.IsNil ? string.Empty : reader.GetString(type.Namespace);
        string name = reader.GetString(type.Name);
        return ns.Length == 0 ? name : $"{ns}.{name}";
    }
}
