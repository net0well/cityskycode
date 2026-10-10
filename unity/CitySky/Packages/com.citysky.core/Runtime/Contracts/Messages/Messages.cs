using System;
using System.Collections.Generic;

namespace CitySky.Contracts
{
    // F1-61. Mensagens do protocolo: classes imutáveis (C# 9, init). POCOs sem atributos de
    // serialização — a serialização (Newtonsoft, TypeNameHandling.None) fica nas pontas.

    /// <summary>Nomes de tipo do envelope (campo <c>type</c>).</summary>
    public static class MessageType
    {
        public const string Hello = "Hello";
        public const string CompileAndRun = "CompileAndRun";
        public const string RunResult = "RunResult";
        public const string Diagnostics = "Diagnostics";
        public const string Cancel = "Cancel";
    }

    /// <summary>Envelope <c>{v, id, type, payload}</c>. Genérico para dar forma de golden JSON (F1-60).</summary>
    public sealed class Envelope<TPayload>
    {
        public int V { get; init; } = Protocol.Version;
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public TPayload? Payload { get; init; }
    }

    /// <summary>Handshake inicial.</summary>
    public sealed class Hello
    {
        public int ProtocolVersion { get; init; } = Protocol.Version;
        public string GameVersion { get; init; } = string.Empty;
    }

    public enum ExecutionMode
    {
        Script = 0,
        Challenge = 1,
        Behavior = 2,
    }

    /// <summary>Orçamentos de execução (CODE_EXECUTION §7).</summary>
    public sealed class Budgets
    {
        public int TimeMs { get; init; }
        public long BudgetTicks { get; init; }
        public int RecursionDepth { get; init; }
        public int MemoryMegabytes { get; init; }
    }

    /// <summary>Pedido de compilar e executar o código do jogador.</summary>
    public sealed class CompileAndRun
    {
        public string RunId { get; init; } = string.Empty;
        public ExecutionMode Mode { get; init; }
        public string EntryPoint { get; init; } = "Program";
        public string Source { get; init; } = string.Empty;
        public Budgets Budgets { get; init; } = new Budgets();
    }

    /// <summary>Métricas de uma execução.</summary>
    public sealed class RunMetrics
    {
        public int CompileMs { get; init; }
        public int RunMs { get; init; }
        public int CommandCount { get; init; }
    }

    /// <summary>Resultado de uma execução: comandos emitidos, console, métricas e erro opcional.</summary>
    public sealed class RunResult
    {
        public string RunId { get; init; } = string.Empty;
        public bool Success { get; init; }
        public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();
        public string Console { get; init; } = string.Empty;
        public string? Error { get; init; }
        public RunMetrics Metrics { get; init; } = new RunMetrics();
    }

    public enum DiagnosticSeverity
    {
        Hidden = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    /// <summary>Diagnóstico de compilação (código CS/CSK, posição, mensagem pt-BR).</summary>
    public sealed class Diagnostic
    {
        public string Id { get; init; } = string.Empty;
        public DiagnosticSeverity Severity { get; init; }
        public string Message { get; init; } = string.Empty;
        public int StartLine { get; init; }
        public int StartColumn { get; init; }
        public int EndLine { get; init; }
        public int EndColumn { get; init; }
    }

    /// <summary>Conjunto de diagnósticos (sublinhados ao digitar).</summary>
    public sealed class Diagnostics
    {
        public IReadOnlyList<Diagnostic> Items { get; init; } = Array.Empty<Diagnostic>();
    }

    /// <summary>Cancelar uma execução em andamento.</summary>
    public sealed class Cancel
    {
        public string RunId { get; init; } = string.Empty;
    }
}
