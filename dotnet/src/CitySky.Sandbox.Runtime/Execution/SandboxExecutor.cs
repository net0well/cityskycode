using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace CitySky.Runtime;

/// <summary>
/// Carrega e executa o assembly do jogador num <see cref="AssemblyLoadContext"/> coletável, com o
/// console redirecionado para um buffer limitado e o orçamento (<see cref="__Budget"/>) armado. O ALC
/// isola carga/descarga (não é a fronteira de segurança — essa é o processo Worker no Job Object, F1-51).
/// </summary>
public static class SandboxExecutor
{
    public static SandboxResult Run(byte[] assembly, byte[] pdb, SandboxBudgets budgets)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var console = new LimitedWriter(budgets.OutputLimitChars);
        var loadWatch = Stopwatch.StartNew();
        var context = new StudentLoadContext();
        var contextRef = new WeakReference(context);
        var status = SandboxStatus.Ok;
        string? reason = null;
        string? exceptionType = null;
        string? message = null;
        int line = 0;
        double loadMs = 0;
        double runMs = 0;

        try
        {
            Assembly loaded = context.LoadFromStream(new MemoryStream(assembly), new MemoryStream(pdb ?? Array.Empty<byte>()));
            MethodInfo entry = loaded.EntryPoint ?? throw new InvalidOperationException("Sem ponto de entrada.");
            loadMs = loadWatch.Elapsed.TotalMilliseconds;

            TextWriter previous = Console.Out;
            Console.SetOut(console);
            __Budget.Reset(budgets.BudgetTicks, budgets.MaxDepth, budgets.AllocationLimitBytes);
            var runWatch = Stopwatch.StartNew();
            try
            {
                object?[]? args = entry.GetParameters().Length == 0 ? null : new object?[] { Array.Empty<string>() };
                entry.Invoke(null, args);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                (status, reason, exceptionType, message, line) = Classify(tie.InnerException, loaded);
            }
            finally
            {
                runMs = runWatch.Elapsed.TotalMilliseconds;
                Console.SetOut(previous);
            }

            if (status == SandboxStatus.Ok && __Budget.Exceeded)
            {
                status = SandboxStatus.Budget;
                reason = __Budget.Reason.ToString();
                line = __Budget.Line;
            }
        }
        catch (Exception ex)
        {
            status = SandboxStatus.Invalid;
            message = ex.Message;
        }

        context.Unload();
        for (int i = 0; i < 3 && contextRef.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        return new SandboxResult(status, reason, line, exceptionType, message, console.ToString(), console.Truncated, loadMs, runMs);
    }

    private static (SandboxStatus, string?, string?, string?, int) Classify(Exception ex, Assembly student)
    {
        if (ex is BudgetExceededException budget)
        {
            return (SandboxStatus.Budget, budget.Reason.ToString(), null, null, budget.Line);
        }

        if (ex is OutOfMemoryException or InsufficientExecutionStackException)
        {
            return (SandboxStatus.Budget, nameof(BudgetReason.Memory), ex.GetType().Name, ex.Message, __Budget.LastLine);
        }

        int line = 0;
        foreach (StackFrame frame in new StackTrace(ex, fNeedFileInfo: true).GetFrames())
        {
            if (frame.GetMethod()?.Module.Assembly == student && frame.GetFileLineNumber() > 0)
            {
                line = frame.GetFileLineNumber();
                break;
            }
        }

        return (SandboxStatus.Exception, null, ex.GetType().FullName, ex.Message, line);
    }

    private sealed class StudentLoadContext : AssemblyLoadContext
    {
        public StudentLoadContext()
            : base("student", isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // A Player API vem da pasta do Worker (assembly real). BCL e runtime de orçamento vêm do
            // contexto padrão (null), para os tipos (__Budget, City) unificarem com os do host.
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

    private sealed class LimitedWriter : TextWriter
    {
        private readonly StringBuilder _buffer = new();
        private readonly int _limit;

        public LimitedWriter(int limit)
        {
            _limit = limit;
        }

        public bool Truncated { get; private set; }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            if (_buffer.Length < _limit)
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
}
