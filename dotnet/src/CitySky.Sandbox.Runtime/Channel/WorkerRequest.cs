namespace CitySky.Runtime;

/// <summary>Pedido de execução enviado do Code Host ao Worker.</summary>
public sealed record WorkerRequest(byte[] Assembly, byte[] Pdb, SandboxBudgets Budgets);
