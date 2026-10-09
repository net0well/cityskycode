using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using CitySky.Spike.Launcher;
using Microsoft.Win32.SafeHandles;

const long MemoryLimitBytes = 256L * 1024 * 1024;
_ = 0; // cap calculado dinamicamente
const int WallTimeoutMs = 15000;

if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("Este spike é específico do Windows.");
    return 1;
}

string restrictedDll = FindRestricted();
string dotnet = FindDotnet();
string outPath = GetArg("--out") ?? Path.Combine(AppContext.BaseDirectory, "s6-results.json");

// Cada caso: ação + o que se espera do Job Object (sandbox do MVP).
// O Job Object contém processos-filhos, memória e CPU, e isola a UI. Ele NÃO restringe sistema de
// arquivos nem rede — isso é a camada AppContainer (Fase 9). Marcamos esses casos como "precisa de AppContainer".
var cases = new (string Action, string Expect)[]
{
    ("ping", "allowed"),
    ("read-allowed", "allowed"),
    ("start-process", "contained"),       // limite de 1 processo ativo
    ("memory", "contained"),              // limite de memória do processo
    ("cpu", "allowed-but-capped"),        // limite de taxa de CPU
    ("read-userfile", "needs-appcontainer"),
    ("write-userfile", "needs-appcontainer"),
    ("http", "needs-appcontainer"),
    ("tcp-loopback", "needs-appcontainer"),
};

// Cria um arquivo-canário na pasta do usuário para o caso read-userfile tentar ler.
string canary = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "s6-canario.txt");
File.WriteAllText(canary, "conteudo secreto do usuario (canario do S6)");

var results = new JsonObject
{
    ["runtime"] = RuntimeInformation.FrameworkDescription,
    ["os"] = RuntimeInformation.OSDescription,
    ["jobLimits"] = new JsonObject
    {
        ["memoryMB"] = MemoryLimitBytes / (1024 * 1024),
        ["cpuHardCapPercentOfSystem"] = Math.Round(Math.Max(100u, (uint)(60.0 / Environment.ProcessorCount * 100)) / 100.0, 2),
        ["affinity"] = "1 processador lógico (garante ≤ 1 núcleo)",
        ["activeProcessLimit"] = 1,
        ["killOnJobClose"] = true,
        ["processorCount"] = Environment.ProcessorCount,
        ["uiRestrictions"] = "desktop, clipboard, handles, atoms globais, parâmetros do sistema, exibição, sair do Windows",
    },
    ["elevated"] = IsElevated(),
};

var array = new JsonArray();
foreach (var c in cases)
{
    array.Add(RunCase(c.Action, c.Expect, restrictedDll, dotnet));
}

results["cases"] = array;

string json = results.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outPath, json);
Console.WriteLine(json);
return 0;

static JsonObject RunCase(string action, string expect, string restrictedDll, string dotnet)
{
    IntPtr job = CreateJob();
    var obj = new JsonObject { ["action"] = action, ["expect"] = expect };

    // Pipe anônimo para capturar a saída do filho.
    var sa = new Native.SecurityAttributes { Length = Marshal.SizeOf<Native.SecurityAttributes>(), InheritHandle = 1 };
    if (!Native.CreatePipe(out IntPtr readPipe, out IntPtr writePipe, ref sa, 0))
    {
        obj["error"] = "CreatePipe falhou";
        Native.CloseHandle(job);
        return obj;
    }

    Native.SetHandleInformation(readPipe, Native.HandleFlagInherit, 0); // o filho não herda a ponta de leitura

    string baseDir = Path.GetDirectoryName(restrictedDll)!;
    string commandLine = $"\"{dotnet}\" \"{restrictedDll}\" {action} \"{baseDir}\"";
    var si = new Native.Startupinfo
    {
        Cb = Marshal.SizeOf<Native.Startupinfo>(),
        Flags = Native.StartfUseStdHandles,
        StdOutput = writePipe,
        StdError = writePipe,
        StdInput = IntPtr.Zero,
    };

    bool created = Native.CreateProcessW(
        dotnet, commandLine, IntPtr.Zero, IntPtr.Zero, inheritHandles: true,
        Native.CreateSuspended | Native.CreateNoWindow | Native.CreateUnicodeEnvironment,
        IntPtr.Zero, baseDir, ref si, out Native.ProcessInformation pi);

    if (!created)
    {
        obj["error"] = $"CreateProcess falhou (Win32 {Marshal.GetLastWin32Error()})";
        Native.CloseHandle(readPipe);
        Native.CloseHandle(writePipe);
        Native.CloseHandle(job);
        return obj;
    }

    bool assigned = Native.AssignProcessToJobObject(job, pi.Process);
    Native.IsProcessInJob(pi.Process, job, out bool inJob);
    Native.ResumeThread(pi.Thread);

    // Fecha a nossa cópia da ponta de escrita, para o EOF chegar quando o filho sair.
    Native.CloseHandle(writePipe);

    string output;
    using (var reader = new StreamReader(new FileStream(new SafeFileHandle(readPipe, ownsHandle: true), FileAccess.Read)))
    {
        output = reader.ReadToEnd().Trim();
    }

    uint wait = Native.WaitForSingleObject(pi.Process, WallTimeoutMs);
    bool timedOut = wait != 0;
    if (timedOut)
    {
        Native.TerminateProcess(pi.Process, 1);
    }

    Native.GetExitCodeProcess(pi.Process, out uint exitCode);

    // Tempo de CPU consumido pelo Job (contabilidade), para o caso da taxa de CPU.
    (double cpuSeconds, uint terminated) = JobAccounting(job);

    Native.CloseHandle(pi.Thread);
    Native.CloseHandle(pi.Process);
    Native.CloseHandle(job); // KILL_ON_JOB_CLOSE encerra qualquer processo que ainda esteja no Job

    string result = ParseResult(output);
    bool killed = result == "NONE" && (exitCode != 0 || timedOut || terminated > 0);

    obj["assignedToJob"] = assigned && inJob;
    obj["childResult"] = result;
    obj["childDetail"] = ParseDetail(output);
    obj["exitCode"] = exitCode;
    obj["killedByJob"] = killed;
    obj["cpuSeconds"] = Math.Round(cpuSeconds, 3);
    obj["verdict"] = Verdict(expect, result, killed, cpuSeconds);
    return obj;
}

static string Verdict(string expect, string result, bool killed, double cpuSeconds) => expect switch
{
    "allowed" => result == "ALLOWED" ? "✓ PASSOU" : $"✗ FALHOU ({result})",
    "contained" => result == "DENIED" || killed ? "✓ PASSOU (contido)" : $"✗ FALHOU (não contido: {result})",
    // Afinidade a 1 núcleo garante ≤ 1 núcleo (≤ ~3 s de CPU em 3 s de parede); o cap de taxa reduz mais.
    "allowed-but-capped" => result == "ALLOWED" && cpuSeconds < 3.3 ? $"✓ PASSOU (CPU {cpuSeconds:F2}s em 3s de parede, ≤ 1 núcleo por afinidade)" : $"✗ FALHOU (CPU {cpuSeconds:F2}s > 1 núcleo, {result})",
    "needs-appcontainer" => result == "ALLOWED" ? "⚠ NÃO contido pelo Job (precisa de AppContainer, Fase 9)" : $"ℹ {result}",
    _ => "?",
};

static IntPtr CreateJob()
{
    IntPtr job = Native.CreateJobObjectW(IntPtr.Zero, IntPtr.Zero);

    var ext = new Native.JobExtendedLimitInformation();
    ext.BasicLimitInformation.LimitFlags =
        Native.JobLimitKillOnJobClose | Native.JobLimitActiveProcess |
        Native.JobLimitProcessMemory | Native.JobLimitDieOnUnhandledException |
        Native.JobLimitAffinity;
    ext.BasicLimitInformation.ActiveProcessLimit = 1;
    // Afinidade a 1 processador lógico: garantia estrutural de ≤ 1 núcleo, independente da máquina.
    ext.BasicLimitInformation.Affinity = (nuint)1;
    ext.ProcessMemoryLimit = (nuint)MemoryLimitBytes;
    SetJobInfo(job, Native.JobObjectExtendedLimitInformation, ext);

    // Cap de taxa de CPU: dimensionado para "morder" nesta máquina (o cap é sobre o total do sistema,
    // então numa máquina de N núcleos 1 thread já usa só ~100/N%). No produto, dimensionar ao hardware
    // ou confiar na afinidade acima.
    uint cpuRate = Math.Max(100u, (uint)(60.0 / Environment.ProcessorCount * 100));
    var cpu = new Native.JobCpuRateControlInformation
    {
        ControlFlags = Native.JobCpuRateEnable | Native.JobCpuRateHardCap,
        CpuRate = cpuRate, // 1/100 de 1%
    };
    SetJobInfo(job, Native.JobObjectCpuRateControlInformation, cpu);

    var ui = new Native.JobBasicUiRestrictions
    {
        UIRestrictionsClass = Native.UiDesktop | Native.UiDisplaySettings | Native.UiExitWindows |
            Native.UiGlobalAtoms | Native.UiHandles | Native.UiReadClipboard |
            Native.UiWriteClipboard | Native.UiSystemParameters,
    };
    SetJobInfo(job, Native.JobObjectBasicUiRestrictions, ui);

    return job;
}

static void SetJobInfo<T>(IntPtr job, int infoClass, T value) where T : struct
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

static (double Seconds, uint Terminated) JobAccounting(IntPtr job)
{
    int size = Marshal.SizeOf<Native.JobBasicAccountingInformation>();
    IntPtr ptr = Marshal.AllocHGlobal(size);
    try
    {
        if (!Native.QueryInformationJobObject(job, Native.JobObjectBasicAccountingInformation, ptr, size, IntPtr.Zero))
        {
            return (0, 0);
        }

        var info = Marshal.PtrToStructure<Native.JobBasicAccountingInformation>(ptr);
        double seconds = (info.TotalUserTime + info.TotalKernelTime) / 10_000_000.0;
        return (seconds, info.TotalTerminatedProcesses);
    }
    finally
    {
        Marshal.FreeHGlobal(ptr);
    }
}

static string ParseResult(string output)
{
    int i = output.IndexOf("RESULT=", StringComparison.Ordinal);
    if (i < 0)
    {
        return "NONE";
    }

    string rest = output[(i + 7)..];
    int sp = rest.IndexOf(' ');
    return sp < 0 ? rest : rest[..sp];
}

static string ParseDetail(string output)
{
    int i = output.IndexOf("DETAIL=", StringComparison.Ordinal);
    return i < 0 ? output : output[(i + 7)..];
}

static bool IsElevated()
{
    using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
    var principal = new System.Security.Principal.WindowsPrincipal(identity);
    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
}

static string? GetArg(string name)
{
    string[] args = Environment.GetCommandLineArgs();
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

static string FindRestricted()
{
    string baseDir = AppContext.BaseDirectory;
    string dll = baseDir.Replace("CitySky.Spike.Launcher", "CitySky.Spike.Restricted");
    string path = Path.Combine(dll, "CitySky.Spike.Restricted.dll");
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Restricted não encontrado em {path}. Compile a solução.");
    }

    return path;
}

static string FindDotnet()
{
    string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    string root = Path.GetFullPath(Path.Combine(coreDir, "..", "..", ".."));
    string dotnet = Path.Combine(root, "dotnet.exe");
    return File.Exists(dotnet) ? dotnet : "dotnet";
}
