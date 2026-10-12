using System;
using System.IO;
using CitySky.Runtime;

// Worker: lê pedidos de execução por stdin e responde por stdout (canal binário), um de cada vez,
// até o stdin fechar. O stdout é o canal do protocolo; a saída de console do jogador vai para um
// buffer limitado dentro do SandboxExecutor (nunca para o stdout bruto). F1-72.
Stream input = Console.OpenStandardInput();
Stream output = Console.OpenStandardOutput();

// Qualquer escrita perdida no Console vai para o vazio, para não corromper o canal.
Console.SetOut(TextWriter.Null);

// Carrega o runtime de orçamento no contexto padrão antes do primeiro pedido, para o assembly do
// jogador resolvê-lo aqui (e não uma segunda cópia).
__Budget.Reset(0, 0, 0);

while (WorkerChannel.ReadRequest(input) is { } request)
{
    SandboxResult result = SandboxExecutor.Run(request.Assembly, request.Pdb, request.Budgets);
    WorkerChannel.WriteResponse(output, result);
}

return 0;
