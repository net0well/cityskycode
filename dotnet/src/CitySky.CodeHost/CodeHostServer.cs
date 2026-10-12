using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitySky.CodeAnalysis;
using CitySky.Contracts;
using CitySky.Runtime;
using Newtonsoft.Json.Linq;

namespace CitySky.CodeHost;

/// <summary>
/// Servidor do protocolo Unity ↔ Code Host sobre um fluxo (named pipe no produto): lê quadros de envelope,
/// despacha por tipo e responde. v0 trata Hello, CompileAndRun e Cancel (F1-70).
/// </summary>
public sealed class CodeHostServer
{
    private const string GameVersion = "0.1.0-skeleton";

    private readonly RunCoordinator _coordinator;

    public CodeHostServer(RunCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    /// <summary>Atende um cliente até o fluxo fechar ou o cancelamento ser pedido.</summary>
    public async Task ServeAsync(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] request;
            try
            {
                if (!MessageFraming.TryReadFrame(stream, out request))
                {
                    return; // cliente desconectou limpo
                }
            }
            catch (IOException)
            {
                return; // pipe quebrado
            }

            byte[]? response = await Task.Run(() => Dispatch(request), cancellationToken).ConfigureAwait(false);
            if (response is not null)
            {
                MessageFraming.WriteFrame(stream, response);
            }
        }
    }

    /// <summary>Processa um quadro e devolve a resposta (ou null quando não há resposta).</summary>
    public byte[]? Dispatch(byte[] request)
    {
        (int _, string id, string type, JObject? payload) = ProtocolSerializer.ReadEnvelope(request);

        switch (type)
        {
            case MessageType.Hello:
                return ProtocolSerializer.Serialize(new Envelope<Hello>
                {
                    Id = id,
                    Type = MessageType.Hello,
                    Payload = new Hello { ProtocolVersion = Protocol.Version, GameVersion = GameVersion },
                });

            case MessageType.CompileAndRun:
                CompileAndRun run = ProtocolSerializer.ReadPayload<CompileAndRun>(payload)
                    ?? new CompileAndRun();
                return ProtocolSerializer.Serialize(new Envelope<RunResult>
                {
                    Id = id,
                    Type = MessageType.RunResult,
                    Payload = Execute(run),
                });

            case MessageType.Cancel:
                return null; // v0: sem fila concorrente; nada a cancelar

            default:
                return null;
        }
    }

    private RunResult Execute(CompileAndRun run)
    {
        string runId = string.IsNullOrEmpty(run.RunId) ? Guid.NewGuid().ToString("N") : run.RunId;
        Budgets budgets = run.Budgets ?? DefaultBudgets();
        RunOutcome outcome = _coordinator.Run(run.Source, budgets);

        if (!outcome.Compiled)
        {
            return new RunResult
            {
                RunId = runId,
                Success = false,
                Error = DescribeCompileFailure(outcome),
                Metrics = new RunMetrics(),
            };
        }

        SandboxResult exec = outcome.Execution!;
        return new RunResult
        {
            RunId = runId,
            Success = exec.Status == SandboxStatus.Ok,
            Console = exec.Console,
            Error = DescribeRuntime(exec),
            Metrics = new RunMetrics { RunMs = (int)exec.RunMs },
        };
    }

    private static string? DescribeCompileFailure(RunOutcome outcome)
    {
        if (outcome.Findings.Count > 0)
        {
            Finding f = outcome.Findings[0];
            return $"{f.Code} (linha {f.Line}): {f.Message}";
        }

        return outcome.Diagnostics.Count > 0 ? outcome.Diagnostics[0] : $"Falha em {outcome.Stage}.";
    }

    private static string? DescribeRuntime(SandboxResult exec) => exec.Status switch
    {
        SandboxStatus.Ok => null,
        SandboxStatus.Budget => $"Orçamento estourado ({exec.Reason})"
            + (exec.Line > 0 ? " na linha " + exec.Line.ToString(CultureInfo.InvariantCulture) : string.Empty) + ".",
        SandboxStatus.Exception => $"{exec.ExceptionType}: {exec.Message}"
            + (exec.Line > 0 ? " (linha " + exec.Line.ToString(CultureInfo.InvariantCulture) + ")" : string.Empty),
        _ => exec.Message,
    };

    private static Budgets DefaultBudgets() => new()
    {
        TimeMs = 2000,
        BudgetTicks = 5_000_000,
        RecursionDepth = 256,
        MemoryMegabytes = 256,
    };
}
