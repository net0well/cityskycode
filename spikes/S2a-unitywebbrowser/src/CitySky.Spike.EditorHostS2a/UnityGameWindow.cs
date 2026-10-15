using System.Diagnostics;

namespace CitySky.Spike.EditorHost;

/// <summary>
/// Lança o player Unity do spike (processo separado, como o jogo no produto) e espera a janela principal.
/// No produto o HWND vem da própria Unity; aqui o host o obtém por <c>MainWindowHandle</c>.
/// </summary>
internal sealed class UnityGameWindow : IDisposable
{
    private readonly Process _process;

    private UnityGameWindow(Process process, IntPtr handle)
    {
        _process = process;
        Handle = handle;
    }

    public IntPtr Handle { get; }

    public int ProcessId => _process.Id;

    public static UnityGameWindow Start(string exePath, TimeSpan timeout)
    {
        string full = Path.GetFullPath(exePath);
        var info = new ProcessStartInfo(full)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(full) ?? string.Empty,
        };
        Process process = Process.Start(info) ?? throw new InvalidOperationException("Não abriu o player Unity.");

        var deadline = Stopwatch.StartNew();
        while (deadline.Elapsed < timeout)
        {
            process.Refresh();
            IntPtr handle = process.MainWindowHandle;
            // A janela do player pode nascer com um título vazio por um instante; exige visível e com classe.
            if (handle != IntPtr.Zero && Native.IsWindowVisible(handle) && Native.ClassName(handle).Length > 0)
            {
                // Dá um tempo para o swapchain começar a apresentar frames.
                Thread.Sleep(1200);
                return new UnityGameWindow(process, handle);
            }

            Thread.Sleep(100);
        }

        try { process.Kill(); } catch { /* já saiu */ }
        throw new TimeoutException("A janela do player Unity não apareceu.");
    }

    public void Close()
    {
        if (!_process.HasExited)
        {
            _process.CloseMainWindow();
            if (!_process.WaitForExit(3000))
            {
                _process.Kill();
            }
        }
    }

    public void Dispose()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }
        }
        catch
        {
            // já saiu
        }

        _process.Dispose();
    }
}
