using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using CitySky.Contracts;

namespace CitySky.Infrastructure
{
    /// <summary>
    /// Cliente da Unity para o Code Host (F1-74): lança o Host (CreateProcess nativo), conecta por named
    /// pipe <b>síncrono</b> (PipeOptions.None — o assíncrono quebra no IL2CPP, spike S1b), faz o handshake
    /// Hello e envia CompileAndRun. No produto fica atrás de IIpcTransport + reinício com backoff.
    /// </summary>
    public sealed class CodeHostClient : ICodeHost, IDisposable
    {
        private NativeProcess _host;
        private NamedPipeClientStream _pipe;

        /// <summary>Inicia o Code Host e conecta. <paramref name="hostExe"/> é o .exe publicado.</summary>
        public void Start(string hostExe, int connectTimeoutMs = 15000)
        {
            if (string.IsNullOrEmpty(hostExe))
            {
                throw new ArgumentNullException(nameof(hostExe));
            }

            string pipeName = "citysky-codehost-" + Guid.NewGuid().ToString("N");
            _host = NativeProcess.Start(hostExe, "--pipe " + pipeName);

            _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
            _pipe.Connect(connectTimeoutMs); // espera o Host criar o pipe (após o aquecimento do Roslyn)

            // Handshake.
            Envelope<Hello> reply = Exchange<Hello, Hello>(MessageType.Hello,
                new Hello { ProtocolVersion = Protocol.Version, GameVersion = "unity" });
            if (reply.Payload == null || reply.Payload.ProtocolVersion != Protocol.Version)
            {
                throw new InvalidOperationException("Handshake com o Code Host falhou (versão do protocolo).");
            }
        }

        /// <summary>Compila e executa o código do jogador; devolve o resultado (console, erro, métricas).</summary>
        public RunResult Run(string source) => Run(source, DefaultBudgets());

        public RunResult Run(string source, Budgets budgets)
        {
            string runId = Guid.NewGuid().ToString("N");
            Envelope<RunResult> reply = Exchange<CompileAndRun, RunResult>(MessageType.CompileAndRun,
                new CompileAndRun
                {
                    RunId = runId,
                    Mode = ExecutionMode.Script,
                    Source = source ?? string.Empty,
                    Budgets = budgets,
                });

            return reply.Payload ?? new RunResult { RunId = runId, Success = false, Error = "Sem resposta do Code Host." };
        }

        /// <summary>Diagnósticos ao digitar (sublinhados): compila sem executar. F1-93.</summary>
        public IReadOnlyList<Diagnostic> GetDiagnostics(string source)
        {
            Envelope<Diagnostics> reply = Exchange<GetDiagnostics, Diagnostics>(MessageType.GetDiagnostics,
                new GetDiagnostics { Source = source ?? string.Empty });
            return reply.Payload?.Items ?? Array.Empty<Diagnostic>();
        }

        private Envelope<TReply> Exchange<TRequest, TReply>(string type, TRequest payload)
        {
            var request = new Envelope<TRequest> { Id = Guid.NewGuid().ToString("N"), Type = type, Payload = payload };
            byte[] bytes = Encoding.UTF8.GetBytes(ProtocolJson.Serialize(request));
            MessageFraming.WriteFrame(_pipe, bytes);

            if (!MessageFraming.TryReadFrame(_pipe, out byte[] replyBytes))
            {
                throw new InvalidOperationException("O Code Host fechou a conexão sem responder.");
            }

            return ProtocolJson.Deserialize<TReply>(Encoding.UTF8.GetString(replyBytes));
        }

        private static Budgets DefaultBudgets() => new Budgets
        {
            TimeMs = 2000,
            BudgetTicks = 5_000_000,
            RecursionDepth = 256,
            MemoryMegabytes = 256,
        };

        public void Dispose()
        {
            try { _pipe?.Dispose(); } catch { /* pipe já fechado */ }
            try { _host?.Dispose(); } catch { /* host já morto */ } // Dispose mata o processo do Host
        }
    }
}
