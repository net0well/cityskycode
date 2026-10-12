using System;
using System.IO;

namespace CitySky.Contracts
{
    /// <summary>
    /// Enquadramento <c>[uint32 length little-endian][bytes]</c> com limite de 8 MB. Lógica pura de bytes,
    /// sem dependências (O6); a serialização JSON fica nas pontas (Code Host / Infrastructure). F1-60.
    /// </summary>
    public static class MessageFraming
    {
        /// <summary>Escreve um quadro. Lança se exceder <see cref="Protocol.MaxMessageBytes"/>.</summary>
        public static void WriteFrame(Stream stream, ReadOnlySpan<byte> payload)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (payload.Length > Protocol.MaxMessageBytes)
            {
                throw new InvalidDataException($"Mensagem de {payload.Length} B excede o limite de {Protocol.MaxMessageBytes} B.");
            }

            Span<byte> header = stackalloc byte[4];
            uint len = (uint)payload.Length;
            header[0] = (byte)len;
            header[1] = (byte)(len >> 8);
            header[2] = (byte)(len >> 16);
            header[3] = (byte)(len >> 24);
            stream.Write(header);
            stream.Write(payload);
            stream.Flush();
        }

        /// <summary>
        /// Lê um quadro. Devolve <c>false</c> quando o fluxo fecha limpo antes do próximo quadro.
        /// Lança em quadro truncado no meio ou em tamanho acima do limite.
        /// </summary>
        public static bool TryReadFrame(Stream stream, out byte[] payload)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            payload = Array.Empty<byte>();

            Span<byte> header = stackalloc byte[4];
            if (!ReadExactlyOrEof(stream, header))
            {
                return false; // fim limpo
            }

            uint len = (uint)(header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24));
            if (len > Protocol.MaxMessageBytes)
            {
                throw new InvalidDataException($"Quadro anuncia {len} B, acima do limite de {Protocol.MaxMessageBytes} B.");
            }

            var body = new byte[len];
            if (len > 0 && !ReadExactly(stream, body))
            {
                throw new EndOfStreamException("Quadro truncado: corpo incompleto.");
            }

            payload = body;
            return true;
        }

        private static bool ReadExactly(Stream stream, Span<byte> buffer)
        {
            int read = 0;
            while (read < buffer.Length)
            {
                int n = stream.Read(buffer.Slice(read));
                if (n == 0)
                {
                    return false;
                }

                read += n;
            }

            return true;
        }

        private static bool ReadExactlyOrEof(Stream stream, Span<byte> buffer)
        {
            int first = stream.Read(buffer);
            if (first == 0)
            {
                return false; // EOF logo no início do quadro = fim limpo
            }

            int read = first;
            while (read < buffer.Length)
            {
                int n = stream.Read(buffer.Slice(read));
                if (n == 0)
                {
                    throw new EndOfStreamException("Cabeçalho de quadro truncado.");
                }

                read += n;
            }

            return true;
        }
    }
}
