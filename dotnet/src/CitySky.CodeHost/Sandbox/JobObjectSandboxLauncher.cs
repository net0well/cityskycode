using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CitySky.CodeHost;

/// <summary>
/// Lançador de produção (Windows): põe o Worker num Job Object que o contém — limite de processos
/// (não cria filhos), limite de memória, afinidade a 1 núcleo, morre em exceção não tratada, restrições
/// de UI e <c>KILL_ON_JOB_CLOSE</c> (o Worker morre com o Host, mesmo travado). F1-51 / ADR-0006.
/// O Job NÃO restringe arquivo/rede — isso é o AppContainer (Fase 9). stdin/stdout por pipes anônimos.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class JobObjectSandboxLauncher : ISandboxLauncher
{
    private const string WorkerName = "CitySky.Sandbox.Worker";

    private readonly string _baseDirectory;
    private readonly long _memoryLimitBytes;

    public JobObjectSandboxLauncher(string? baseDirectory = null, long memoryLimitBytes = 256L * 1024 * 1024)
    {
        _baseDirectory = baseDirectory ?? AppContext.BaseDirectory;
        _memoryLimitBytes = memoryLimitBytes;
    }

    public ISandboxProcess Launch()
    {
        (string? applicationName, string commandLine) = ResolveWorker();

        IntPtr job = CreateConfiguredJob();
        var sa = new Native.SecurityAttributes { Length = Marshal.SizeOf<Native.SecurityAttributes>(), InheritHandle = 1 };

        if (!Native.CreatePipe(out IntPtr stdinRead, out IntPtr stdinWrite, ref sa, 0)
            || !Native.CreatePipe(out IntPtr stdoutRead, out IntPtr stdoutWrite, ref sa, 0))
        {
            Native.CloseHandle(job);
            throw new InvalidOperationException($"CreatePipe falhou: Win32 {Marshal.GetLastWin32Error()}");
        }

        // As pontas do Host não são herdadas pelo filho (para o EOF chegar quando um lado fecha).
        Native.SetHandleInformation(stdinWrite, Native.HandleFlagInherit, 0);
        Native.SetHandleInformation(stdoutRead, Native.HandleFlagInherit, 0);

        var si = new Native.Startupinfo
        {
            Cb = Marshal.SizeOf<Native.Startupinfo>(),
            Flags = Native.StartfUseStdHandles,
            StdInput = stdinRead,
            StdOutput = stdoutWrite,
            StdError = stdoutWrite,
        };

        bool created = Native.CreateProcessW(
            applicationName, commandLine, IntPtr.Zero, IntPtr.Zero, inheritHandles: true,
            Native.CreateSuspended | Native.CreateNoWindow | Native.CreateUnicodeEnvironment,
            IntPtr.Zero, _baseDirectory, ref si, out Native.ProcessInformation pi);

        if (!created)
        {
            int err = Marshal.GetLastWin32Error();
            CloseAll(job, stdinRead, stdinWrite, stdoutRead, stdoutWrite);
            throw new InvalidOperationException($"CreateProcess do Worker falhou: Win32 {err}");
        }

        // Prende no Job ANTES de rodar, para os limites valerem desde o primeiro instante.
        Native.AssignProcessToJobObject(job, pi.Process);
        Native.ResumeThread(pi.Thread);

        // O Host fica com a escrita de stdin e a leitura de stdout; fecha as pontas do filho.
        Native.CloseHandle(stdinRead);
        Native.CloseHandle(stdoutWrite);

        return new JobObjectSandboxProcess(job, pi.Process, pi.Thread, stdinWrite, stdoutRead);
    }

    private (string? ApplicationName, string CommandLine) ResolveWorker()
    {
        string exe = Path.Combine(_baseDirectory, WorkerName + ".exe");
        if (File.Exists(exe))
        {
            return (exe, $"\"{exe}\"");
        }

        string dll = Path.Combine(_baseDirectory, WorkerName + ".dll");
        if (File.Exists(dll))
        {
            return ("dotnet", $"dotnet \"{dll}\"");
        }

        throw new FileNotFoundException($"Worker não encontrado em {_baseDirectory} ({WorkerName}.exe/.dll).");
    }

    private IntPtr CreateConfiguredJob()
    {
        IntPtr job = Native.CreateJobObjectW(IntPtr.Zero, IntPtr.Zero);
        if (job == IntPtr.Zero)
        {
            throw new InvalidOperationException($"CreateJobObject falhou: Win32 {Marshal.GetLastWin32Error()}");
        }

        var ext = default(Native.JobExtendedLimitInformation);
        ext.BasicLimitInformation.LimitFlags =
            Native.JobLimitKillOnJobClose | Native.JobLimitActiveProcess |
            Native.JobLimitProcessMemory | Native.JobLimitDieOnUnhandledException |
            Native.JobLimitAffinity;
        ext.BasicLimitInformation.ActiveProcessLimit = 1;      // o Worker não cria processos-filho
        ext.BasicLimitInformation.Affinity = (nuint)1;         // 1 processador lógico → ≤ 1 núcleo
        ext.ProcessMemoryLimit = (nuint)_memoryLimitBytes;
        SetJobInfo(job, Native.JobObjectExtendedLimitInformation, ext);

        var ui = new Native.JobBasicUiRestrictions
        {
            UIRestrictionsClass = Native.UiDesktop | Native.UiDisplaySettings | Native.UiExitWindows |
                Native.UiGlobalAtoms | Native.UiHandles | Native.UiReadClipboard |
                Native.UiWriteClipboard | Native.UiSystemParameters,
        };
        SetJobInfo(job, Native.JobObjectBasicUiRestrictions, ui);

        return job;
    }

    private static void SetJobInfo<T>(IntPtr job, int infoClass, T value)
        where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, ptr, false);
            if (!Native.SetInformationJobObject(job, infoClass, ptr, size))
            {
                throw new InvalidOperationException($"SetInformationJobObject({infoClass}) falhou: Win32 {Marshal.GetLastWin32Error()}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private static void CloseAll(params IntPtr[] handles)
    {
        foreach (IntPtr h in handles)
        {
            if (h != IntPtr.Zero)
            {
                Native.CloseHandle(h);
            }
        }
    }
}
