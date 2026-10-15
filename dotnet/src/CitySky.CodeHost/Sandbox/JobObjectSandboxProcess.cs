using System;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace CitySky.CodeHost;

/// <summary>
/// Processo Worker rodando dentro de um Job Object, com stdin/stdout ligados por pipes anônimos.
/// Descartar fecha o Job → <c>KILL_ON_JOB_CLOSE</c> encerra o Worker mesmo se ele travar (F1-51).
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class JobObjectSandboxProcess : ISandboxProcess
{
    private readonly IntPtr _job;
    private readonly IntPtr _process;
    private readonly IntPtr _thread;
    private readonly FileStream _stdin;
    private readonly FileStream _stdout;
    private bool _disposed;

    public JobObjectSandboxProcess(IntPtr job, IntPtr process, IntPtr thread, IntPtr stdinWrite, IntPtr stdoutRead)
    {
        _job = job;
        _process = process;
        _thread = thread;
        _stdin = new FileStream(new SafeFileHandle(stdinWrite, ownsHandle: true), FileAccess.Write);
        _stdout = new FileStream(new SafeFileHandle(stdoutRead, ownsHandle: true), FileAccess.Read);
    }

    public Stream StandardInput => _stdin;

    public Stream StandardOutput => _stdout;

    public bool HasExited =>
        Native.GetExitCodeProcess(_process, out uint code) && code != Native.StillActive;

    public void Kill()
    {
        try
        {
            if (!HasExited)
            {
                Native.TerminateProcess(_process, 1);
            }
        }
        catch
        {
            // já encerrado
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try { _stdin.Dispose(); } catch { /* pipe já fechado */ }
        try { _stdout.Dispose(); } catch { /* pipe já fechado */ }

        Kill();
        if (_thread != IntPtr.Zero) { Native.CloseHandle(_thread); }
        if (_process != IntPtr.Zero) { Native.CloseHandle(_process); }
        // Fechar o Job encerra qualquer processo que ainda esteja nele (KILL_ON_JOB_CLOSE).
        if (_job != IntPtr.Zero) { Native.CloseHandle(_job); }
    }
}
