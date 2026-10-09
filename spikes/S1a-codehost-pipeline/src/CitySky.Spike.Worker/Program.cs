using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using CitySky.Runtime;
using CitySky.Spike.Contracts;

// Worker: lê pedidos por stdin e responde por stdout, um de cada vez, até o stdin fechar.
// O stdout é o canal do protocolo; a saída de console do aluno vai para um buffer limitado.
Stream input = Console.OpenStandardInput();
Stream output = Console.OpenStandardOutput();

// O protocolo usa o stdout bruto; qualquer escrita perdida no Console vai para o vazio,
// para não corromper o canal.
Console.SetOut(TextWriter.Null);

// Carrega o runtime de orçamento no contexto padrão antes do primeiro pedido,
// para o assembly do aluno resolvê-lo aqui (e não carregar uma segunda cópia).
__Budget.Reset(0, 0, 0);

while (Frame.Read<RunRequest>(input) is { } request)
{
    Frame.Write(output, Execute(request));
}

static RunResponse Execute(RunRequest request)
{
    var console = new LimitedWriter(request.OutputLimitChars);
    var loadWatch = Stopwatch.StartNew();
    var context = new StudentLoadContext();
    WeakReference contextRef = new(context);
    string status = "ok";
    string? reason = null;
    string? exceptionType = null;
    string? message = null;
    int line = 0;
    double loadMs = 0;
    double runMs = 0;

    try
    {
        Assembly assembly = context.LoadFromStream(new MemoryStream(request.Assembly), new MemoryStream(request.Pdb));
        MethodInfo entry = assembly.EntryPoint ?? throw new InvalidOperationException("Sem ponto de entrada.");
        loadMs = loadWatch.Elapsed.TotalMilliseconds;

        Console.SetOut(console);
        __Budget.Reset(request.BudgetTicks, request.MaxDepth, request.AllocationLimitBytes);
        var runWatch = Stopwatch.StartNew();
        try
        {
            object?[]? args = entry.GetParameters().Length == 0 ? null : [Array.Empty<string>()];
            entry.Invoke(null, args);
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            (status, reason, exceptionType, message, line) = Classify(tie.InnerException, assembly);
        }
        finally
        {
            runMs = runWatch.Elapsed.TotalMilliseconds;
            Console.SetOut(TextWriter.Null);
        }

        // Mesmo que o aluno tenha capturado algo, um estouro marcado vale.
        if (status == "ok" && __Budget.Exceeded)
        {
            status = "budget";
            reason = __Budget.Reason.ToString();
            line = __Budget.Line;
        }
    }
    catch (Exception ex)
    {
        Console.SetOut(TextWriter.Null);
        status = "invalid";
        message = ex.Message;
    }

    context.Unload();
    for (int i = 0; i < 3 && contextRef.IsAlive; i++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    using Process self = Process.GetCurrentProcess();
    return new RunResponse(
        status, reason, line, exceptionType, message,
        console.ToString(), console.Truncated,
        loadMs, runMs,
        self.WorkingSet64, GC.GetTotalMemory(false));
}

static (string Status, string? Reason, string? Type, string? Message, int Line) Classify(Exception ex, Assembly student)
{
    if (ex is BudgetExceededException budget)
    {
        return ("budget", budget.Reason.ToString(), null, null, budget.Line);
    }

    if (ex is OutOfMemoryException or InsufficientExecutionStackException)
    {
        return ("budget", nameof(BudgetReason.Memory), ex.GetType().Name, ex.Message, __Budget.LastLine);
    }

    // Linha original: primeiro quadro da pilha que pertence ao assembly do aluno (PDB em memória).
    int line = 0;
    foreach (StackFrame frame in new StackTrace(ex, fNeedFileInfo: true).GetFrames())
    {
        if (frame.GetMethod()?.Module.Assembly == student && frame.GetFileLineNumber() > 0)
        {
            line = frame.GetFileLineNumber();
            break;
        }
    }

    return ("exception", null, ex.GetType().FullName, ex.Message, line);
}

/// <summary>Contexto coletável: isola carga e descarga (não é fronteira de segurança).</summary>
internal sealed class StudentLoadContext() : AssemblyLoadContext("student", isCollectible: true)
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // A Player API vem da pasta do Worker (como seria um assembly real no produto).
        // A BCL e o runtime de orçamento vêm do contexto padrão (null), para os tipos unificarem.
        if (assemblyName.Name == "CitySky.PlayerApi")
        {
            string path = Path.Combine(AppContext.BaseDirectory, "CitySky.PlayerApi.dll");
            if (File.Exists(path))
            {
                return LoadFromAssemblyPath(path);
            }
        }

        return null;
    }
}

/// <summary>Saída de console com limite de caracteres.</summary>
internal sealed class LimitedWriter(int limit) : TextWriter
{
    private readonly StringBuilder _buffer = new();

    public bool Truncated { get; private set; }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        if (_buffer.Length < limit)
        {
            _buffer.Append(value);
        }
        else
        {
            Truncated = true;
        }
    }

    public override string ToString() => _buffer.ToString();
}
