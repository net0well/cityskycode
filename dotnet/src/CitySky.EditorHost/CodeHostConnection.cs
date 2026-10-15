using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using CitySky.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CitySky.EditorHost;

/// <summary>
/// Conexão do host do editor com o Code Host (named pipe). O host do editor é .NET, então pode usar
/// <see cref="Process.Start"/>; as mesmas opções de serialização do protocolo (golden) valem.
/// </summary>
public sealed class CodeHostConnection : IDisposable
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None,
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
    };

    private readonly Process _host;
    private readonly NamedPipeClientStream _pipe;

    public CodeHostConnection(string hostExe, int connectTimeoutMs = 15000)
    {
        string pipeName = "citysky-editorhost-" + Guid.NewGuid().ToString("N");
        var info = new ProcessStartInfo(hostExe, "--pipe " + pipeName)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(hostExe) ?? string.Empty,
        };
        _host = Process.Start(info) ?? throw new InvalidOperationException("Não abriu o Code Host.");

        _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
        _pipe.Connect(connectTimeoutMs);

        Envelope<Hello> hello = Exchange<Hello, Hello>(MessageType.Hello,
            new Hello { ProtocolVersion = Protocol.Version, GameVersion = "editor-host" });
        if (hello.Payload is null || hello.Payload.ProtocolVersion != Protocol.Version)
        {
            throw new InvalidOperationException("Handshake com o Code Host falhou.");
        }
    }

    public RunResult Run(string source)
    {
        string id = Guid.NewGuid().ToString("N");
        return Exchange<CompileAndRun, RunResult>(MessageType.CompileAndRun, new CompileAndRun
        {
            RunId = id,
            Mode = ExecutionMode.Script,
            Source = source,
            Budgets = new Budgets { TimeMs = 2000, BudgetTicks = 5_000_000, RecursionDepth = 256, MemoryMegabytes = 256 },
        }).Payload ?? new RunResult { RunId = id, Success = false, Error = "Sem resposta do Code Host." };
    }

    public IReadOnlyList<Diagnostic> GetDiagnostics(string source) =>
        Exchange<GetDiagnostics, Diagnostics>(MessageType.GetDiagnostics, new GetDiagnostics { Source = source })
            .Payload?.Items ?? Array.Empty<Diagnostic>();

    private Envelope<TReply> Exchange<TRequest, TReply>(string type, TRequest payload)
    {
        var request = new Envelope<TRequest> { Id = Guid.NewGuid().ToString("N"), Type = type, Payload = payload };
        byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request, Settings));
        MessageFraming.WriteFrame(_pipe, bytes);

        if (!MessageFraming.TryReadFrame(_pipe, out byte[] replyBytes))
        {
            throw new InvalidOperationException("O Code Host fechou a conexão sem responder.");
        }

        return JsonConvert.DeserializeObject<Envelope<TReply>>(Encoding.UTF8.GetString(replyBytes), Settings)!;
    }

    public void Dispose()
    {
        try { _pipe.Dispose(); } catch { /* pipe fechado */ }
        try { if (!_host.HasExited) { _host.Kill(entireProcessTree: true); } } catch { /* já morto */ }
        _host.Dispose();
    }
}
