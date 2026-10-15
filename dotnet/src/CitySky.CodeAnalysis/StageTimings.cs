namespace CitySky.CodeAnalysis;

/// <summary>Tempos por estágio do pipeline (ms).</summary>
public sealed record StageTimings(double ParseMs, double L1Ms, double RewriteMs, double CompileMs, double EmitMs, double L2Ms)
{
    public double TotalMs => ParseMs + L1Ms + RewriteMs + CompileMs + EmitMs + L2Ms;
}
