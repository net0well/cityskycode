using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace CitySky.Spike.EditorHost;

public sealed record CompletionItemDto(string Label, string Kind);

public sealed record DiagnosticDto(string Id, string Message, string Severity, int StartLine, int StartColumn, int EndLine, int EndColumn);

/// <summary>
/// Serviços de linguagem sobre um AdhocWorkspace: projeto da Player API (stub) + projeto do aluno.
/// Cada pedido cria um snapshot imutável do documento (sem aplicar mudanças ao workspace).
/// </summary>
public sealed class RoslynService
{
    private static readonly string[] ReferenceNames =
        ["System.Private.CoreLib.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll", "System.Console.dll"];

    private readonly AdhocWorkspace _workspace;
    private readonly DocumentId _programId;

    public RoslynService()
    {
        _workspace = new AdhocWorkspace(MefHostServices.Create(MefHostServices.DefaultAssemblies));
        var references = LoadReferences();
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp14, DocumentationMode.Parse);

        var apiProject = ProjectInfo.Create(
            ProjectId.CreateNewId(), VersionStamp.Create(), "CitySky.PlayerApi", "CitySky.PlayerApi", LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable),
            parseOptions: parseOptions,
            metadataReferences: references);
        _workspace.AddProject(apiProject);
        _workspace.AddDocument(apiProject.Id, "PlayerApi.cs", SourceText.From(PlayerApiStub.Source));

        var studentProject = ProjectInfo.Create(
            ProjectId.CreateNewId(), VersionStamp.Create(), "Program", "Program", LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.ConsoleApplication, nullableContextOptions: NullableContextOptions.Enable),
            parseOptions: parseOptions,
            metadataReferences: references,
            projectReferences: [new ProjectReference(apiProject.Id)]);
        _workspace.AddProject(studentProject);
        _programId = _workspace.AddDocument(studentProject.Id, "Program.cs", SourceText.From(string.Empty)).Id;
    }

    public async Task<(IReadOnlyList<CompletionItemDto> Items, double ServerMs)> GetCompletionsAsync(string text, int offset, CancellationToken cancellationToken = default)
    {
        long start = Stopwatch.GetTimestamp();
        Document document = Snapshot(text);
        CompletionService service = CompletionService.GetService(document)
            ?? throw new InvalidOperationException("CompletionService indisponível (MEF sem Features).");
        CompletionList list = await service.GetCompletionsAsync(document, offset, cancellationToken: cancellationToken);

        var items = new List<CompletionItemDto>(list.ItemsList.Count);
        foreach (CompletionItem item in list.ItemsList)
        {
            items.Add(new CompletionItemDto(item.DisplayText, item.Tags.IsEmpty ? string.Empty : item.Tags[0]));
        }

        return (items, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
    }

    public async Task<(IReadOnlyList<DiagnosticDto> Items, double ServerMs)> GetDiagnosticsAsync(string text, CancellationToken cancellationToken = default)
    {
        long start = Stopwatch.GetTimestamp();
        Document document = Snapshot(text);
        SemanticModel model = await document.GetSemanticModelAsync(cancellationToken)
            ?? throw new InvalidOperationException("Sem modelo semântico.");

        var items = new List<DiagnosticDto>();
        foreach (Diagnostic diagnostic in model.GetDiagnostics(cancellationToken: cancellationToken))
        {
            if (diagnostic.Severity < DiagnosticSeverity.Warning || !diagnostic.Location.IsInSource)
            {
                continue;
            }

            FileLinePositionSpan span = diagnostic.Location.GetLineSpan();
            items.Add(new DiagnosticDto(
                diagnostic.Id,
                diagnostic.GetMessage(),
                diagnostic.Severity.ToString(),
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                span.EndLinePosition.Line + 1,
                span.EndLinePosition.Character + 1));
        }

        return (items, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
    }

    private Document Snapshot(string text) =>
        _workspace.CurrentSolution.GetDocument(_programId)!.WithText(SourceText.From(text));

    private static List<MetadataReference> LoadReferences()
    {
        string tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("TRUSTED_PLATFORM_ASSEMBLIES indisponível.");

        var references = new List<MetadataReference>();
        foreach (string path in tpa.Split(Path.PathSeparator))
        {
            if (ReferenceNames.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }
}
