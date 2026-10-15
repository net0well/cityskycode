namespace CitySky.Runtime;

/// <summary>Resultado de uma execução isolada.</summary>
public sealed record SandboxResult(
    SandboxStatus Status,
    string? Reason,
    int Line,
    string? ExceptionType,
    string? Message,
    string Console,
    bool Truncated,
    double LoadMs,
    double RunMs);
