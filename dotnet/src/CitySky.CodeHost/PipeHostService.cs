using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using CitySky.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CitySky.CodeHost;

/// <summary>
/// Serviço hospedado: aquece o Roslyn, abre o named pipe da sessão e atende a Unity, reabrindo para o
/// próximo cliente quando um desconecta, até o host parar (F1-70).
/// </summary>
public sealed class PipeHostService : BackgroundService
{
    private readonly CodeHostServer _server;
    private readonly Pipeline _pipeline;
    private readonly PipeConfig _config;
    private readonly ILogger<PipeHostService> _logger;

    public PipeHostService(CodeHostServer server, Pipeline pipeline, PipeConfig config, ILogger<PipeHostService> logger)
    {
        _server = server;
        _pipeline = pipeline;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aquecimento: carrega o Roslyn com uma compilação "dummy" (JIT) antes do primeiro pedido real.
        _ = _pipeline.Compile("System.Console.WriteLine(1);");

        string pipeName = _config.Name ?? "citysky-codehost-" + Guid.NewGuid().ToString("N");
        _logger.LogInformation("Code Host pronto. Pipe: {Pipe}", pipeName);
        Console.WriteLine("CODEHOST_PIPE=" + pipeName); // o lançador/Unity lê isto quando não passou --pipe

        while (!stoppingToken.IsCancellationRequested)
        {
            using var pipe = new NamedPipeServerStream(
                pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try
            {
                await pipe.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await _server.ServeAsync(pipe, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Conexão encerrada com erro; reabrindo o pipe.");
            }
        }
    }
}
