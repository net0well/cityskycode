using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CitySky.CodeAnalysis;

/// <summary>Um achado de análise (código pedagógico CSK, mensagem pt-BR, linha).</summary>
public sealed record Finding(string Code, string Message, int Line);

/// <summary>
/// L1 (CODE_EXECUTION_SECURITY.md): percorre a árvore do jogador e, pelo modelo semântico, confere cada
/// tipo e membro referenciado contra a <see cref="Allowlist"/>. Também barra construções sintáticas
/// proibidas (unsafe, ponteiros, finalizadores, ModuleInitializer, extern/DllImport).
/// </summary>
public sealed class SymbolChecker
{
    private readonly SemanticModel _model;
    private readonly List<Finding> _findings = new();

    private SymbolChecker(SemanticModel model) => _model = model;

    public static IReadOnlyList<Finding> Check(SemanticModel model, SyntaxNode root)
    {
        var checker = new SymbolChecker(model);
        checker.VisitSyntax(root);
        checker.VisitSymbols(root);
        return checker._findings;
    }

    private void VisitSyntax(SyntaxNode root)
    {
        foreach (SyntaxNode node in root.DescendantNodes(descendIntoTrivia: false))
        {
            switch (node)
            {
                case UnsafeStatementSyntax:
                case PointerTypeSyntax:
                case FunctionPointerTypeSyntax:
                case StackAllocArrayCreationExpressionSyntax:
                    Report("CSK1004", "Recurso de baixo nível não está disponível na cidade.", node);
                    break;
                case DestructorDeclarationSyntax:
                    Report("CSK1002", "Finalizadores não estão disponíveis na cidade.", node);
                    break;
                case LocalFunctionStatementSyntax { Modifiers: var m } when m.Any(SyntaxKind.ExternKeyword):
                case MethodDeclarationSyntax { Modifiers: var mm } when mm.Any(SyntaxKind.ExternKeyword):
                    Report("CSK1004", "Métodos externos não estão disponíveis na cidade.", node);
                    break;
                case AttributeSyntax attr when IsBannedAttribute(attr):
                    Report("CSK1003", "Este atributo não está disponível na cidade.", node);
                    break;
            }
        }
    }

    private static bool IsBannedAttribute(AttributeSyntax attr)
    {
        string name = attr.Name.ToString();
        return name.Contains("ModuleInitializer") || name.Contains("DllImport") || name.Contains("UnmanagedCallersOnly");
    }

    private void VisitSymbols(SyntaxNode root)
    {
        foreach (SyntaxNode node in root.DescendantNodes(descendIntoTrivia: false))
        {
            switch (node)
            {
                case MemberAccessExpressionSyntax:
                case ObjectCreationExpressionSyntax:
                case IdentifierNameSyntax:
                case GenericNameSyntax:
                    CheckSymbol(node);
                    break;
            }
        }
    }

    private void CheckSymbol(SyntaxNode node)
    {
        SymbolInfo info = _model.GetSymbolInfo(node);
        ISymbol? symbol = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
        if (symbol is null)
        {
            return;
        }

        switch (symbol)
        {
            case IMethodSymbol method:
                CheckMember(method.ContainingType, method.Name, node);
                break;
            case IPropertySymbol property:
                CheckMember(property.ContainingType, property.Name, node);
                break;
            case IFieldSymbol field:
                CheckMember(field.ContainingType, field.Name, node);
                break;
            case INamedTypeSymbol type:
                CheckType(type, node);
                break;
        }
    }

    private void CheckMember(INamedTypeSymbol? containingType, string memberName, SyntaxNode node)
    {
        if (containingType is null || IsStudentType(containingType))
        {
            return;
        }

        string typeName = Canonical(containingType);
        if (!Allowlist.IsMemberAllowed(typeName, memberName))
        {
            Report("CSK1001", $"'{typeName}.{memberName}' não está disponível na cidade.", node);
        }
    }

    private void CheckType(INamedTypeSymbol type, SyntaxNode node)
    {
        if (IsStudentType(type) || type.TypeKind == TypeKind.TypeParameter)
        {
            return;
        }

        string name = Canonical(type);
        if (!Allowlist.IsTypeAllowed(name))
        {
            Report("CSK1001", $"O tipo '{name}' não está disponível na cidade.", node);
        }
    }

    private static bool IsStudentType(INamedTypeSymbol type) =>
        type.Locations.Any(l => l.IsInSource) && type.ContainingAssembly?.Name is not "CitySky.PlayerApi";

    /// <summary>Forma canônica "Namespace.Tipo" com aridade (ex.: System.Collections.Generic.List`1).</summary>
    private static string Canonical(INamedTypeSymbol type)
    {
        INamedTypeSymbol definition = type.OriginalDefinition;
        string ns = definition.ContainingNamespace is { IsGlobalNamespace: false } n ? n.ToDisplayString() + "." : string.Empty;
        string name = definition.Name;
        return definition.Arity > 0 ? $"{ns}{name}`{definition.Arity}" : $"{ns}{name}";
    }

    private void Report(string code, string message, SyntaxNode node)
    {
        int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        if (!_findings.Any(f => f.Code == code && f.Line == line && f.Message == message))
        {
            _findings.Add(new Finding(code, message, line));
        }
    }
}
