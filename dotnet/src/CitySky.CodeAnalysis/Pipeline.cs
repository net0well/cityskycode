using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace CitySky.CodeAnalysis;

/// <summary>Pipeline do Code Host: Parse → L1 → Reescrita → Compilação → Emissão → L2 (F1-71).</summary>
public sealed class Pipeline
{
    private static readonly CSharpParseOptions Parse = new(LanguageVersion.CSharp14);
    private readonly ReferenceSet _references;

    public Pipeline(ReferenceSet references) => _references = references;

    /// <param name="enforceL1">quando falso, pula o bloqueio de L1 (para provar que L2 pega sozinho).</param>
    public CompileResult Compile(string source, bool enforceL1 = true)
    {
        double parseMs, l1Ms = 0, rewriteMs = 0, compileMs = 0, emitMs = 0, l2Ms = 0;

        long t = Stopwatch.GetTimestamp();
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, Parse, path: "Program.cs", encoding: Encoding.UTF8);
        parseMs = Ms(ref t);

        var syntaxErrors = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(Describe).ToList();

        var options = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication,
            // Debug: sem otimização, os pontos de sequência do PDB mapeiam a linha exata do erro
            // (feedback pedagógico). Custo desprezível em programas pequenos.
            optimizationLevel: OptimizationLevel.Debug,
            allowUnsafe: false,
            deterministic: true);

        CSharpCompilation compilation = CSharpCompilation.Create("StudentProgram", new[] { tree }, _references.References, options);

        var semanticErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(Describe).ToList();

        if (syntaxErrors.Count > 0)
        {
            return Fail("syntax", syntaxErrors, parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms);
        }

        // L1: lista de permissões por símbolo.
        t = Stopwatch.GetTimestamp();
        SemanticModel model = compilation.GetSemanticModel(tree);
        IReadOnlyList<Finding> l1 = SymbolChecker.Check(model, tree.GetRoot());
        l1Ms = Ms(ref t);

        if (semanticErrors.Count > 0)
        {
            return Fail("compile", semanticErrors, parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms, l1);
        }

        if (enforceL1 && l1.Count > 0)
        {
            return new CompileResult(false, "L1", l1, Array.Empty<string>(), null, null,
                new StageTimings(parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms));
        }

        // Reescrita de orçamento.
        t = Stopwatch.GetTimestamp();
        SyntaxNode rewritten = BudgetRewriter.Rewrite(tree.GetRoot());
        SyntaxTree rewrittenTree = CSharpSyntaxTree.Create((Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode)rewritten, Parse, "Program.cs", Encoding.UTF8);
        rewriteMs = Ms(ref t);

        // Recompila a árvore instrumentada.
        t = Stopwatch.GetTimestamp();
        CSharpCompilation finalCompilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(rewrittenTree);
        var rewriteErrors = finalCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error).Select(Describe).ToList();
        compileMs = Ms(ref t);
        if (rewriteErrors.Count > 0)
        {
            // Erro aqui = bug na reescrita; reportamos para corrigir.
            return Fail("compile", rewriteErrors.Select(e => "[instrumentado] " + e).ToList(),
                parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms, l1);
        }

        // Emissão (PE + PDB portátil, determinístico).
        t = Stopwatch.GetTimestamp();
        using var peStream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        EmitResult emit = finalCompilation.Emit(peStream, pdbStream,
            options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
        emitMs = Ms(ref t);
        if (!emit.Success)
        {
            return Fail("compile", emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(Describe).ToList(),
                parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms, l1);
        }

        byte[] peBytes = peStream.ToArray();
        byte[] pdbBytes = pdbStream.ToArray();

        // L2: verificação do IL emitido.
        t = Stopwatch.GetTimestamp();
        IReadOnlyList<Finding> l2 = IlVerifier.Verify(peBytes);
        l2Ms = Ms(ref t);
        if (l2.Count > 0)
        {
            return new CompileResult(false, "L2", l2, Array.Empty<string>(), null, null,
                new StageTimings(parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms));
        }

        return new CompileResult(true, "emitted", l1, Array.Empty<string>(), peBytes, pdbBytes,
            new StageTimings(parseMs, l1Ms, rewriteMs, compileMs, emitMs, l2Ms));
    }

    private static CompileResult Fail(string stage, IReadOnlyList<string> diagnostics,
        double parse, double l1, double rewrite, double compile, double emit, double l2,
        IReadOnlyList<Finding>? findings = null) =>
        new(false, stage, findings ?? Array.Empty<Finding>(), diagnostics, null, null,
            new StageTimings(parse, l1, rewrite, compile, emit, l2));

    private static string Describe(Diagnostic d)
    {
        int line = d.Location.IsInSource ? d.Location.GetLineSpan().StartLinePosition.Line + 1 : 0;
        return $"{d.Id} (linha {line}): {d.GetMessage(CultureInfo.InvariantCulture)}";
    }

    private static double Ms(ref long since)
    {
        double ms = Stopwatch.GetElapsedTime(since).TotalMilliseconds;
        since = Stopwatch.GetTimestamp();
        return ms;
    }
}
