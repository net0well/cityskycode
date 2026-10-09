using System;

namespace CitySky.Runtime;

/// <summary>Motivo pelo qual o orçamento estourou.</summary>
public enum BudgetReason
{
    None = 0,
    Loop = 1,
    Recursion = 2,
    Memory = 3,
    Output = 4,
}

/// <summary>
/// Orçamento estourado. O aluno não consegue "engolir" esta exceção: o estado fica marcado como
/// estourado e qualquer chamada seguinte ao orçamento lança de novo; os <c>catch</c> do aluno
/// recebem um filtro que não deixa capturá-la (CODE_EXECUTION_SECURITY.md, L3).
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

/// <summary>
/// Chamadas injetadas pelo reescritor do Code Host. O código do aluno não pode chamá-las diretamente:
/// a allowlist (L1) barra o tipo, e só o código gerado pelo reescritor o referencia.
/// Uma execução por vez por processo Worker, então estado estático é aceitável aqui.
/// </summary>
public static class __Budget
{
    private const int AllocationCheckInterval = 16;

    private static long s_remainingTicks;
    private static int s_depth;
    private static int s_maxDepth;
    private static long s_allocationLimit;
    private static long s_allocationStart;
    private static int s_sinceAllocationCheck;

    public static bool Exceeded { get; private set; }

    public static BudgetReason Reason { get; private set; }

    public static int Line { get; private set; }

    public static int LastLine { get; private set; }

    public static void Reset(long ticks, int maxDepth, long allocationLimitBytes)
    {
        s_remainingTicks = ticks;
        s_depth = 0;
        s_maxDepth = maxDepth;
        s_allocationLimit = allocationLimitBytes;
        s_allocationStart = GC.GetAllocatedBytesForCurrentThread();
        s_sinceAllocationCheck = 0;
        Exceeded = false;
        Reason = BudgetReason.None;
        Line = 0;
        LastLine = 0;
    }

    /// <summary>Início de cada iteração de laço.</summary>
    public static void Tick(int line)
    {
        LastLine = line;
        if (Exceeded)
        {
            throw new BudgetExceededException(Reason, Line);
        }

        if (--s_remainingTicks < 0)
        {
            Fail(BudgetReason.Loop, line);
        }

        CheckAllocation(line);
    }

    /// <summary>Entrada de método, função local, lambda ou acessor.</summary>
    public static void Enter(int line)
    {
        LastLine = line;
        if (Exceeded)
        {
            throw new BudgetExceededException(Reason, Line);
        }

        if (++s_depth > s_maxDepth)
        {
            Fail(BudgetReason.Recursion, line);
        }

        if (--s_remainingTicks < 0)
        {
            Fail(BudgetReason.Loop, line);
        }

        CheckAllocation(line);
    }

    public static void Exit()
    {
        s_depth--;
    }

    private static void CheckAllocation(int line)
    {
        if (++s_sinceAllocationCheck < AllocationCheckInterval)
        {
            return;
        }

        s_sinceAllocationCheck = 0;
        if (GC.GetAllocatedBytesForCurrentThread() - s_allocationStart > s_allocationLimit)
        {
            Fail(BudgetReason.Memory, line);
        }
    }

    private static void Fail(BudgetReason reason, int line)
    {
        Exceeded = true;
        Reason = reason;
        Line = line;
        throw new BudgetExceededException(reason, line);
    }
}
