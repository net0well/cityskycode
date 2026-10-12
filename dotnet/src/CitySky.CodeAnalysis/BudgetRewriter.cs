using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CitySky.CodeAnalysis;

/// <summary>
/// L3 (CODE_EXECUTION_SECURITY.md). Insere, sem alterar a contagem de linhas do original:
///  • <c>__Budget.Tick(linha)</c> no início de cada corpo de laço (iterações, memória);
///  • <c>__Budget.Enter(linha)</c> + <c>try … finally __Budget.Exit()</c> em corpos de método/função
///    local/acessor/construtor (profundidade, para parar a recursão antes da pilha nativa);
///  • um filtro nos <c>catch</c> para o estouro de orçamento não ser capturado pelo jogador.
/// As linhas vêm da árvore original (passadas como argumento), então são exatas mesmo com a instrumentação.
/// </summary>
public sealed class BudgetRewriter : CSharpSyntaxRewriter
{
    private const string Ns = "global::CitySky.Runtime.__Budget";
    private static readonly SyntaxTriviaList Sp = TriviaList(Space);

    public static SyntaxNode Rewrite(SyntaxNode root) => new BudgetRewriter().Visit(root);

    public override SyntaxNode? VisitForStatement(ForStatementSyntax node) =>
        base.VisitForStatement(node.WithStatement(TickBody(node.Statement, Line(node))));

    public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) =>
        base.VisitWhileStatement(node.WithStatement(TickBody(node.Statement, Line(node))));

    public override SyntaxNode? VisitDoStatement(DoStatementSyntax node) =>
        base.VisitDoStatement(node.WithStatement(TickBody(node.Statement, Line(node))));

    public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) =>
        base.VisitForEachStatement(node.WithStatement(TickBody(node.Statement, Line(node))));

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (node.Body is { } b)
        {
            node = node.WithBody(WrapBody(b, Line(node)));
        }
        else if (node.ExpressionBody is { } e)
        {
            node = node.WithExpressionBody(null).WithSemicolonToken(default)
                .WithBody(WrapBody(ExpressionToBlock(e, IsVoid(node.ReturnType)), Line(node)));
        }

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        if (node.Body is { } b)
        {
            node = node.WithBody(WrapBody(b, Line(node)));
        }
        else if (node.ExpressionBody is { } e)
        {
            node = node.WithExpressionBody(null).WithSemicolonToken(default)
                .WithBody(WrapBody(ExpressionToBlock(e, IsVoid(node.ReturnType)), Line(node)));
        }

        return base.VisitLocalFunctionStatement(node);
    }

    private static bool IsVoid(TypeSyntax returnType) =>
        returnType is PredefinedTypeSyntax p && p.Keyword.IsKind(SyntaxKind.VoidKeyword);

    private static BlockSyntax ExpressionToBlock(ArrowExpressionClauseSyntax arrow, bool isVoid)
    {
        StatementSyntax stmt = isVoid
            ? ExpressionStatement(arrow.Expression)
            : ReturnStatement(arrow.Expression.WithLeadingTrivia(Space));
        return Block(stmt);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node) =>
        base.VisitConstructorDeclaration(node.Body is { } b ? node.WithBody(WrapBody(b, Line(node))) : node);

    public override SyntaxNode? VisitAccessorDeclaration(AccessorDeclarationSyntax node) =>
        base.VisitAccessorDeclaration(node.Body is { } b ? node.WithBody(WrapBody(b, Line(node))) : node);

    public override SyntaxNode? VisitCatchClause(CatchClauseSyntax node)
    {
        var visited = (CatchClauseSyntax)base.VisitCatchClause(node)!;
        string guard = $"!{Ns}.Exceeded";
        ExpressionSyntax condition = visited.Filter is { } f
            ? ParseExpression($"({f.FilterExpression}) && ({guard})")
            : ParseExpression(guard);
        return visited.WithFilter(CatchFilterClause(condition.WithLeadingTrivia(Space)).WithLeadingTrivia(Space).WithTrailingTrivia(Space));
    }

    /// <summary>Garante bloco e prepende o Tick na mesma linha do '{' (sem deslocar as instruções).</summary>
    private static BlockSyntax TickBody(StatementSyntax body, int loopLine)
    {
        BlockSyntax block = body as BlockSyntax ?? Block(body);
        StatementSyntax tick = Stmt($"{Ns}.Tick({loopLine});")
            .WithLeadingTrivia(Space)
            .WithTrailingTrivia(block.OpenBraceToken.TrailingTrivia.Count > 0 ? block.OpenBraceToken.TrailingTrivia : Sp);
        return block
            .WithOpenBraceToken(block.OpenBraceToken.WithTrailingTrivia(Space))
            .WithStatements(block.Statements.Insert(0, tick));
    }

    /// <summary>
    /// { Enter(ln); try &lt;corpo&gt; finally { Exit(); } } reaproveitando as chaves originais para o
    /// bloco do try, de modo que nenhuma linha nova seja criada.
    /// </summary>
    private static BlockSyntax WrapBody(BlockSyntax body, int line)
    {
        StatementSyntax enter = Stmt($"{Ns}.Enter({line});").WithLeadingTrivia(Space).WithTrailingTrivia(Space);
        StatementSyntax exit = Stmt($"{Ns}.Exit();").WithLeadingTrivia(Space).WithTrailingTrivia(Space);

        FinallyClauseSyntax fin = FinallyClause(Block(exit).WithLeadingTrivia(Space).WithTrailingTrivia(Space))
            .WithFinallyKeyword(Token(SyntaxKind.FinallyKeyword).WithLeadingTrivia(Space).WithTrailingTrivia(Space));

        TryStatementSyntax tryStmt = TryStatement(
            Token(SyntaxKind.TryKeyword).WithLeadingTrivia(Space).WithTrailingTrivia(Space),
            body.WithLeadingTrivia(),
            default,
            fin).WithLeadingTrivia(Space);

        return Block(enter, tryStmt)
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(Space))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(Space));
    }

    private static StatementSyntax Stmt(string text) => ParseStatement(text);

    private static int Line(SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
}
