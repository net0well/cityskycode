namespace CitySky.Runtime;

/// <summary>Como terminou uma execução isolada.</summary>
public enum SandboxStatus
{
    Ok = 0,
    Budget = 1,
    Exception = 2,
    Invalid = 3,
}
