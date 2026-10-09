using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CitySky.Spike.EditorHost;

public sealed record ProcessMemory(int Pid, string Name, long WorkingSetBytes, long PrivateBytes);

/// <summary>Memória do host + processos filhos (msedgewebview2.exe), via snapshot ToolHelp.</summary>
internal static class ProcessTree
{
    private const uint Th32csSnapProcess = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ProcessEntry32
    {
        public uint Size;
        public uint Usage;
        public uint ProcessId;
        public IntPtr DefaultHeapId;
        public uint ModuleId;
        public uint Threads;
        public uint ParentProcessId;
        public int PriorityClassBase;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ExeFile;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32FirstW(IntPtr snapshot, ref ProcessEntry32 entry);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32NextW(IntPtr snapshot, ref ProcessEntry32 entry);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    /// <summary>Soma a árvore de <paramref name="rootPid"/>, ignorando a subárvore de <paramref name="excludedPid"/> (o jogo falso).</summary>
    public static IReadOnlyList<ProcessMemory> Measure(int rootPid, int excludedPid)
    {
        var parentOf = new Dictionary<int, int>();
        IntPtr snapshot = CreateToolhelp32Snapshot(Th32csSnapProcess, 0);
        try
        {
            var entry = new ProcessEntry32 { Size = (uint)Marshal.SizeOf<ProcessEntry32>() };
            for (bool ok = Process32FirstW(snapshot, ref entry); ok; ok = Process32NextW(snapshot, ref entry))
            {
                parentOf[(int)entry.ProcessId] = (int)entry.ParentProcessId;
            }
        }
        finally
        {
            CloseHandle(snapshot);
        }

        var tree = new HashSet<int> { rootPid };
        bool grew = true;
        while (grew)
        {
            grew = false;
            foreach ((int pid, int parent) in parentOf)
            {
                if (pid != excludedPid && tree.Contains(parent) && tree.Add(pid))
                {
                    grew = true;
                }
            }
        }

        // O PID do pai pode ter sido reutilizado por outro processo: só contamos o host e o WebView2.
        var result = new List<ProcessMemory>();
        foreach (int pid in tree)
        {
            try
            {
                using Process process = Process.GetProcessById(pid);
                if (pid != rootPid && !process.ProcessName.Equals("msedgewebview2", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(new ProcessMemory(pid, process.ProcessName, process.WorkingSet64, process.PrivateMemorySize64));
            }
            catch (ArgumentException)
            {
                // processo terminou durante a medição
            }
        }

        return result;
    }

    /// <summary>Versão do runtime WebView2 lida do executável de um dos processos filhos.</summary>
    public static string? WebView2Version(IEnumerable<ProcessMemory> processes)
    {
        foreach (ProcessMemory p in processes)
        {
            if (!p.Name.Equals("msedgewebview2", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                using Process process = Process.GetProcessById(p.Pid);
                return process.MainModule?.FileVersionInfo.FileVersion;
            }
            catch (Exception)
            {
                // sem acesso ao módulo; tenta o próximo
            }
        }

        return null;
    }
}
