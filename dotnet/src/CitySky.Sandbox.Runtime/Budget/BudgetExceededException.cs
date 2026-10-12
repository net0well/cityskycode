using System;

namespace CitySky.Runtime;

/// <summary>
/// Orçamento estourado. O jogador não consegue "engolir" esta exceção: o estado fica marcado e qualquer
/// chamada seguinte ao orçamento lança de novo; os <c>catch</c> recebem um filtro que não deixa capturá-la
/// (o rewriter injeta <c>when (!__Budget.Exceeded)</c>).
/// </summary>
public sealed class BudgetExceededException : Exception
{
    public BudgetExceededException(BudgetReason reason, int line)
        : base($"Orçamento estourado ({reason}) na linha {line}.")
    {
        Reason = reason;
        Line = line;
    }

    public BudgetReason Reason { get; }

    public int Line { get; }
}
