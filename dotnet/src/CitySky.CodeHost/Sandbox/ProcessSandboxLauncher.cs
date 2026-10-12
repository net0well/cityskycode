using System;
using System.Diagnostics;
using System.IO;

namespace CitySky.CodeHost;

/// <summary>Lançador v0: <see cref="Process"/> com stdin/stdout redirecionados. Sem API exclusiva de SO.</summary>
public sealed class ProcessSandboxLauncher : ISandboxLauncher
{
    private const string WorkerName = "CitySky.Sandbox.Worker";

    private readonly string _baseDirectory;

    public ProcessSandboxLauncher(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
    }

    public ISandboxProcess Launch()
    {
        string exe = Path.Combine(_baseDirectory, WorkerName + ".exe");
        string dll = Path.Combine(_baseDirectory, WorkerName + ".dll");

        ProcessStartInfo info;
        if (File.Exists(exe))
        {
            info = new ProcessStartInfo(exe);
        }
        else if (File.Exists(dll))
        {
            info = new ProcessStartInfo("dotnet", $"\"{dll}\"");
        }
        else
        {
            throw new FileNotFoundException($"Worker não encontrado em {_baseDirectory} ({WorkerName}.exe/.dll).");
        }

        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        info.WorkingDirectory = _baseDirectory;

        Process process = Process.Start(info) ?? throw new InvalidOperationException("Process.Start devolveu null.");
        return new ProcessSandbox(process);
    }

    private sealed class ProcessSandbox : ISandboxProcess
    {
        private readonly Process _process;

        public ProcessSandbox(Process process)
        {
            _process = process;
        }

        public Stream StandardInput => _process.StandardInput.BaseStream;

        public Stream StandardOutput => _process.StandardOutput.BaseStream;

        public bool HasExited
        {
            get
            {
                try { return _process.HasExited; }
                catch { return true; }
            }
        }

        public void Kill()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // já encerrado
            }
        }

        public void Dispose()
        {
            Kill();
            _process.Dispose();
        }
    }
}
