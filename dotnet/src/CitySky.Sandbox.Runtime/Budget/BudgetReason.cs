namespace CitySky.Runtime;

/// <summary>Motivo pelo qual o orçamento estourou (CODE_EXECUTION_SECURITY.md, L3).</summary>
public enum BudgetReason
{
    None = 0,
    Loop = 1,
    Recursion = 2,
    Memory = 3,
    Output = 4,
}
