using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace CitySky.Spike.CodeHost;

/// <summary>
/// Monta as referências da compilação do aluno: os assemblies de referência da BCL do .NET 10,
/// o stub da Player API e o assembly do runtime de orçamento.
/// Reforça a lição do CODE_EXECUTION.md §4.1: restringir referências NÃO basta (System.IO.File vive
/// em System.Runtime); por isso L1 e L2 filtram por símbolo. Aqui referenciamos a BCL inteira de propósito.
/// </summary>
public sealed class ReferenceSet
{
    private ReferenceSet(IReadOnlyList<MetadataReference> references, string refPackVersion, string playerApiPath)
    {
        References = references;
        RefPackVersion = refPackVersion;
        PlayerApiPath = playerApiPath;
    }

    public IReadOnlyList<MetadataReference> References { get; }

    public string RefPackVersion { get; }

    /// <summary>Caminho do CitySky.PlayerApi.dll emitido. No produto é um assembly real na pasta do Worker.</summary>
    public string PlayerApiPath { get; }

    public static ReferenceSet Create()
    {
        string refDir = FindReferencePack(out string version);
        var references = new List<MetadataReference>();
        foreach (string dll in Directory.EnumerateFiles(refDir, "*.dll"))
        {
            references.Add(MetadataReference.CreateFromFile(dll));
        }

        // Player API (stub): compilada e gravada em disco para o Worker conseguir carregá-la.
        string playerApiPath = Path.Combine(AppContext.BaseDirectory, "CitySky.PlayerApi.dll");
        references.Add(CompilePlayerApi(references, playerApiPath));

        // Runtime de orçamento (injetado pelo rewriter): o assembly real, do diretório do Code Host.
        string runtimePath = Path.Combine(AppContext.BaseDirectory, "CitySky.Spike.Runtime.dll");
        references.Add(MetadataReference.CreateFromFile(runtimePath));

        return new ReferenceSet(references, version, playerApiPath);
    }

    private static MetadataReference CompilePlayerApi(List<MetadataReference> bclReferences, string outputPath)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(PlayerApiStub.Source, new CSharpParseOptions(LanguageVersion.CSharp14));
        var compilation = CSharpCompilation.Create(
            "CitySky.PlayerApi",
            [tree],
            bclReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

        EmitResult result = compilation.Emit(outputPath);
        if (!result.Success)
        {
            throw new InvalidOperationException("Falha ao compilar o stub da Player API: "
                + string.Join("; ", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        }

        return MetadataReference.CreateFromFile(outputPath);
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
