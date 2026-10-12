using System;
using System.IO;
using System.Text;

namespace CitySky.Runtime;

/// <summary>Pedido de execução enviado do Code Host ao Worker.</summary>
public sealed record WorkerRequest(byte[] Assembly, byte[] Pdb, SandboxBudgets Budgets);

/// <summary>
/// Canal binário Host ↔ Worker (sobre stdin/stdout do Worker). Binário porque o assembly é binário;
/// JSON com base64 seria desperdício. Cada mensagem é autodescrita pelos seus comprimentos. F1-72.
/// </summary>
public static class WorkerChannel
{
    /// <summary>Escreve um pedido. Devolve após o flush.</summary>
    public static void WriteRequest(Stream stream, WorkerRequest request)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(request);

        using var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        w.Write((byte)1); // há pedido
        WriteBytes(w, request.Assembly);
        WriteBytes(w, request.Pdb);
        w.Write(request.Budgets.BudgetTicks);
        w.Write(request.Budgets.MaxDepth);
        w.Write(request.Budgets.AllocationLimitBytes);
        w.Write(request.Budgets.OutputLimitChars);
        w.Flush();
    }

    /// <summary>Lê um pedido, ou <c>null</c> quando o fluxo fecha limpo (fim dos pedidos).</summary>
    public static WorkerRequest? ReadRequest(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var r = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        int marker;
        try
        {
            marker = r.Read();
        }
        catch (EndOfStreamException)
        {
            return null;
        }

        if (marker < 0)
        {
            return null; // EOF limpo
        }

        byte[] assembly = ReadBytes(r);
        byte[] pdb = ReadBytes(r);
        long ticks = r.ReadInt64();
        int depth = r.ReadInt32();
        long alloc = r.ReadInt64();
        int output = r.ReadInt32();
        return new WorkerRequest(assembly, pdb, new SandboxBudgets(ticks, depth, alloc, output));
    }

    public static void WriteResponse(Stream stream, SandboxResult result)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(result);

        using var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        w.Write((int)result.Status);
        WriteNullable(w, result.Reason);
        w.Write(result.Line);
        WriteNullable(w, result.ExceptionType);
        WriteNullable(w, result.Message);
        w.Write(result.Console);
        w.Write(result.Truncated);
        w.Write(result.LoadMs);
        w.Write(result.RunMs);
        w.Flush();
    }

    public static SandboxResult ReadResponse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var r = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var status = (SandboxStatus)r.ReadInt32();
        string? reason = ReadNullable(r);
        int line = r.ReadInt32();
        string? exType = ReadNullable(r);
        string? message = ReadNullable(r);
        string console = r.ReadString();
        bool truncated = r.ReadBoolean();
        double loadMs = r.ReadDouble();
        double runMs = r.ReadDouble();
        return new SandboxResult(status, reason, line, exType, message, console, truncated, loadMs, runMs);
    }

    private static void WriteBytes(BinaryWriter w, byte[] data)
    {
        w.Write(data.Length);
        w.Write(data);
    }

    private static byte[] ReadBytes(BinaryReader r)
    {
        int len = r.ReadInt32();
        return len == 0 ? Array.Empty<byte>() : r.ReadBytes(len);
    }

    private static void WriteNullable(BinaryWriter w, string? value)
    {
        w.Write(value is not null);
        if (value is not null)
        {
            w.Write(value);
        }
    }

    private static string? ReadNullable(BinaryReader r) => r.ReadBoolean() ? r.ReadString() : null;
}
