using System.Buffers.Binary;
using System.Text.Json;

namespace CitySky.Spike.Contracts;

/// <summary>Pedido do Code Host ao Worker.</summary>
public sealed record RunRequest(byte[] Assembly, byte[] Pdb, long BudgetTicks, int MaxDepth, long AllocationLimitBytes, int OutputLimitChars);

/// <summary>Resposta do Worker. O Code Host valida tudo (o Worker é tratado como hostil).</summary>
public sealed record RunResponse(
    string Status,          // ok | budget | exception | invalid
    string? Reason,         // Loop | Recursion | Memory (status = budget)
    int Line,
    string? ExceptionType,
    string? Message,
    string Output,
    bool OutputTruncated,
    double LoadMs,
    double RunMs,
    long WorkerWorkingSetBytes,
    long ManagedHeapBytes);

/// <summary>Enquadramento <c>[uint32 tamanho][json UTF-8]</c>, com limite de tamanho.</summary>
public static class Frame
{
    public const int MaxBytes = 32 * 1024 * 1024;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static void Write<T>(Stream stream, T message)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(message, Json);
        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(header, (uint)body.Length);
        stream.Write(header);
        stream.Write(body);
        stream.Flush();
    }

    public static T? Read<T>(Stream stream)
    {
        Span<byte> header = stackalloc byte[4];
        if (!ReadExactly(stream, header))
        {
            return default; // fim do fluxo
        }

        uint length = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (length > MaxBytes)
        {
            throw new InvalidDataException($"Mensagem de {length} bytes excede o limite de {MaxBytes}.");
        }

        byte[] body = new byte[length];
        if (!ReadExactly(stream, body))
        {
            throw new EndOfStreamException("Mensagem truncada.");
        }

        return JsonSerializer.Deserialize<T>(body, Json);
    }

    private static bool ReadExactly(Stream stream, Span<byte> buffer)
    {
        int read = 0;
        while (read < buffer.Length)
        {
            int n = stream.Read(buffer[read..]);
            if (n == 0)
            {
                return false;
            }

            read += n;
        }

        return true;
    }
}
