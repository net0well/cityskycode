using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CitySky.CodeAnalysis;

/// <summary>
/// Monta as referências da compilação do jogador: os assemblies de referência da BCL do .NET 10,
/// a Player API e o runtime de orçamento. Reforça a lição do CODE_EXECUTION.md §4.1: restringir
/// referências NÃO basta (System.IO.File vive em System.Runtime); por isso L1 e L2 filtram por símbolo.
/// A BCL inteira é referenciada de propósito.
/// </summary>
public sealed class ReferenceSet
{
    private ReferenceSet(IReadOnlyList<MetadataReference> references, string refPackVersion)
    {
        References = references;
        RefPackVersion = refPackVersion;
    }

    public IReadOnlyList<MetadataReference> References { get; }

    public string RefPackVersion { get; }

    public static ReferenceSet Create()
    {
        string refDir = FindReferencePack(out string version);
        var references = new List<MetadataReference>();
        foreach (string dll in Directory.EnumerateFiles(refDir, "*.dll"))
        {
            references.Add(MetadataReference.CreateFromFile(dll));
        }

        references.Add(RequireAssembly("CitySky.PlayerApi.dll"));
        references.Add(RequireAssembly("CitySky.Sandbox.Runtime.dll"));
        return new ReferenceSet(references, version);
    }

    private static PortableExecutableReference RequireAssembly(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Assembly necessário não encontrado na pasta do Code Host: {fileName}", path);
        }

        return MetadataReference.CreateFromFile(path);
    }

    private static string FindReferencePack(out string version)
    {
        string dotnetRoot = Path.GetDirectoryName(typeof(object).Assembly.Location)!; // .../shared/Microsoft.NETCore.App/<ver>
        string root = Path.GetFullPath(Path.Combine(dotnetRoot, "..", "..", ".."));
        string packRoot = Path.Combine(root, "packs", "Microsoft.NETCore.App.Ref");
        if (!Directory.Exists(packRoot))
        {
            throw new DirectoryNotFoundException($"Ref pack não encontrado em {packRoot}. Instale o pacote de referência do .NET 10.");
        }

        string best = Directory.EnumerateDirectories(packRoot)
            .OrderByDescending(d => Version.TryParse(Path.GetFileName(d), out Version? v) ? v : new Version(0, 0))
            .First();
        version = Path.GetFileName(best);
        return Directory.EnumerateDirectories(Path.Combine(best, "ref")).OrderByDescending(d => d).First();
    }
}
