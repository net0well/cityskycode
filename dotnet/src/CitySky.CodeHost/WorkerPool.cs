using System;
using System.Threading.Tasks;
using CitySky.Runtime;

namespace CitySky.CodeHost;

/// <summary>
/// Mantém um Worker quente e o recicla após violação, timeout, crash ou N execuções (F1-73).
/// Uma execução por vez (lock). O watchdog de tempo real é a rede de segurança além do orçamento L3:
/// se o Worker não responder no prazo, é morto e trocado.
/// </summary>
public sealed class WorkerPool : IDisposable
{
    private const int MaxRunsBeforeRecycle = 50;

    private readonly ISandboxLauncher _launcher;
    private readonly object _gate = new();
    private ISandboxProcess? _worker;
    private int _runs;
    private bool _disposed;

    public WorkerPool(ISandboxLauncher launcher)
    {
        _launcher = launcher;
    }

    /// <summary>Executa um pedido, com limite de tempo de parede em milissegundos.</summary>
    public SandboxResult Execute(WorkerRequest request, int wallTimeoutMs)
    {
        ArgumentNullException.ThrowIfNull(request);

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            ISandboxProcess worker = EnsureWorker();
            try
            {
                WorkerChannel.WriteRequest(worker.StandardInput, request);

                Task<SandboxResult> read = Task.Run(() => WorkerChannel.ReadResponse(worker.StandardOutput));
                if (!read.Wait(wallTimeoutMs))
                {
                    Recycle();
                    return Timeout();
                }

                SandboxResult result = read.GetAwaiter().GetResult();

                if (++_runs >= MaxRunsBeforeRecycle)
                {
                    Recycle();
                }

                return result;
            }
            catch (Exception ex)
            {
                // Worker caiu no meio (ex.: StackOverflow não capturável): recicla e reporta.
                Recycle();
                return Crashed(ex);
            }
        }
    }

    private ISandboxProcess EnsureWorker()
    {
        if (_worker is null || _worker.HasExited)
        {
            Recycle();
            _worker = _launcher.Launch();
            _runs = 0;
        }

        return _worker;
    }

    private void Recycle()
    {
        try { _worker?.Dispose(); } catch { /* já morto */ }
        _worker = null;
        _runs = 0;
    }

    private static SandboxResult Timeout() => new(
        SandboxStatus.Budget, "Timeout", 0, null,
        "O programa demorou demais e foi interrompido.", string.Empty, false, 0, 0);

    private static SandboxResult Crashed(Exception ex) => new(
        SandboxStatus.Invalid, null, 0, ex.GetType().Name,
        "O programa parou inesperadamente (recursão infinita?).", string.Empty, false, 0, 0);

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            Recycle();
        }
    }
}
