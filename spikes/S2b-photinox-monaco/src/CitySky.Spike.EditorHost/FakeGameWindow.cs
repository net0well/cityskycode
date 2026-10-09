using System.Diagnostics;

namespace CitySky.Spike.EditorHost;

/// <summary>
/// "Janela do jogo" falsa, em **outro processo** (como a Unity no produto): um formulário WinForms
/// aberto pelo PowerShell (<c>fake-game.ps1</c>). Serve para testar o editor embutido entre processos.
/// </summary>
internal sealed class FakeGameWindow : IDisposable
{
    private readonly Process _process;

    private FakeGameWindow(Process process, IntPtr handle)
    {
        _process = process;
        Handle = handle;
    }

    public IntPtr Handle { get; }

    public int ProcessId => _process.Id;

    public static FakeGameWindow Start(TimeSpan timeout)
    {
        string script = Path.Combine(AppContext.BaseDirectory, "fake-game.ps1");
        var info = new ProcessStartInfo("powershell.exe", ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script])
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        Process process = Process.Start(info) ?? throw new InvalidOperationException("Não abriu o PowerShell.");

        var deadline = Stopwatch.StartNew();
        while (deadline.Elapsed < timeout)
        {
            process.Refresh();
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                return new FakeGameWindow(process, process.MainWindowHandle);
            }

            Thread.Sleep(100);
        }

        process.Kill();
        throw new TimeoutException("A janela do jogo falso não apareceu.");
    }

    public void Close()
    {
        if (!_process.HasExited)
        {
            _process.CloseMainWindow();
            _process.WaitForExit(3000);
        }
    }

    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill();
        }

        _process.Dispose();
    }
}
