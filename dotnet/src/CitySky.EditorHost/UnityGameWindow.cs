using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;

namespace CitySky.EditorHost;

/// <summary>
/// Lança o player do jogo (processo separado) e espera a janela principal ficar pronta para receber
/// o editor como painel-filho. No produto final o HWND virá da própria Unity hospedando o host; aqui
/// o host abre o player e descobre o HWND por <see cref="Process.MainWindowHandle"/>.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class UnityGameWindow : IDisposable
{
    private readonly Process _process;

    private UnityGameWindow(Process process, IntPtr handle)
    {
        _process = process;
        Handle = handle;
    }

    public IntPtr Handle { get; }

    public bool IsAlive => !_process.HasExited;

    public static UnityGameWindow Start(string exePath, TimeSpan timeout)
    {
        string full = Path.GetFullPath(exePath);
        var info = new ProcessStartInfo(full)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(full) ?? string.Empty,
        };
        Process process = Process.Start(info) ?? throw new InvalidOperationException("O player do jogo não abriu.");

        var deadline = Stopwatch.StartNew();
        while (deadline.Elapsed < timeout)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"O player do jogo encerrou antes de abrir a janela (código {process.ExitCode}).");
            }

            process.Refresh();
            IntPtr handle = process.MainWindowHandle;
            // A janela do player pode nascer com título vazio por um instante; exige visível e com classe.
            if (handle != IntPtr.Zero && Native.IsWindowVisible(handle) && Native.ClassName(handle).Length > 0)
            {
                Thread.Sleep(1200); // deixa o swapchain começar a apresentar frames
                return new UnityGameWindow(process, handle);
            }

            Thread.Sleep(100);
        }

        try { process.Kill(); } catch { /* já saiu */ }
        throw new TimeoutException("A janela do player do jogo não apareceu dentro do tempo limite.");
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
            // já encerrou
        }

        _process.Dispose();
    }
}
