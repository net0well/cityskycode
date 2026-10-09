using System.Diagnostics;
using CitySky.Spike.Contracts;

namespace CitySky.Spike.CodeHost;

/// <summary>
/// Mantém um processo Worker quente. Envia um pedido, lê a resposta com um cronômetro de parede como
/// rede de segurança (se a instrumentação for contornada, o processo é encerrado). Recicla após
/// violação, falha ou N execuções. No produto, o launcher é o ISandboxLauncher (Job Object + mais).
/// </summary>
public sealed class WorkerPool : IDisposable
{
    private readonly string _workerDll;
    private readonly int _recycleAfter;
    private Process? _process;
    private int _runsOnCurrent;

    public WorkerPool(string workerDll, int recycleAfter = 200)
    {
        _workerDll = workerDll;
        _recycleAfter = recycleAfter;
    }

    public int Restarts { get; private set; }

    public RunResponse Run(RunRequest request, int wallClockMs)
    {
        Process process = EnsureProcess();
        try
        {
            Frame.Write(process.StandardInput.BaseStream, request);

            RunResponse? response = null;
            var reader = Task.Run(() => response = Frame.Read<RunResponse>(process.StandardOutput.BaseStream));
            if (!reader.Wait(wallClockMs))
            {
                Recycle(process);
                return Killed("watchdog", "O programa não respondeu a tempo e foi encerrado (possível laço infinito não instrumentado).");
            }

            if (response is null)
            {
                Recycle(process);
                return Killed("crash", "O programa parou inesperadamente (recursão profunda demais ou falha).");
            }

            _runsOnCurrent++;
            if (response.Status is "budget" or "invalid" || _runsOnCurrent >= _recycleAfter)
            {
                Recycle(process);
            }

            return response;
        }
        catch (IOException)
        {
            Recycle(_process);
            return Killed("crash", "A comunicação com o programa foi perdida (o processo caiu).");
        }
    }

    private Process EnsureProcess()
    {
        if (_process is { HasExited: false })
        {
            return _process;
        }

        var info = new ProcessStartInfo("dotnet", $"\"{_workerDll}\"")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        _process = Process.Start(info) ?? throw new InvalidOperationException("Não foi possível iniciar o Worker.");
        _runsOnCurrent = 0;
        return _process;
    }

    private void Recycle(Process? process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception)
        {
            // já encerrado
        }

        if (ReferenceEquals(process, _process))
        {
            _process = null;
            Restarts++;
        }
    }

    private static RunResponse Killed(string status, string message) =>
        new(status, null, 0, null, message, string.Empty, false, 0, 0, 0, 0);

    public void Dispose() => Recycle(_process);
}
