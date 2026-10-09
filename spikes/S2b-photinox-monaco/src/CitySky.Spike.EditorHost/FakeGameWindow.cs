using System.Diagnostics;

namespace CitySky.Spike.EditorHost;

/// <summary>
/// "Janela do jogo" falsa, em **outro processo** (como a Unity no produto): um formulário WinForms
/// aberto pelo PowerShell. Serve para testar a janela do editor com dono entre processos.
/// </summary>
internal sealed class FakeGameWindow : IDisposable
{
    private const string Script =
        "Add-Type -AssemblyName System.Windows.Forms; " +
        "$f = New-Object System.Windows.Forms.Form; " +
        "$f.Text = 'Jogo falso (spike S2b)'; $f.BackColor = 'DarkSlateGray'; " +
        "$f.StartPosition = 'Manual'; $f.Left = 80; $f.Top = 80; $f.Width = 1280; $f.Height = 720; " +
        "[System.Windows.Forms.Application]::Run($f)";

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
        var info = new ProcessStartInfo("powershell.exe", ["-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", Script])
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
