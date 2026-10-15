using System.Buffers.Binary;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

// Processo de eco do S1b. Dois modos:
//   --pipe <nome>        conecta como cliente de named pipe e ecoa (o player é o servidor)
//   --tcp <arquivoPorta> escuta em 127.0.0.1:0, grava a porta no arquivo e ecoa
// Enquadramento: [uint32 tamanho little-endian][bytes]. Ecoa cada quadro de volta até o fluxo fechar.
try
{
    if (args.Length >= 2 && args[0] == "--pipe")
    {
        using var pipe = new NamedPipeClientStream(".", args[1], PipeDirection.InOut, PipeOptions.Asynchronous);
        pipe.Connect(10000);
        EchoLoop(pipe);
    }
    else if (args.Length >= 2 && args[0] == "--tcp")
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        File.WriteAllText(args[1], port.ToString());
        using TcpClient client = listener.AcceptTcpClient();
        client.NoDelay = true;
        EchoLoop(client.GetStream());
        listener.Stop();
    }
    else
    {
        Console.Error.WriteLine("uso: --pipe <nome> | --tcp <arquivoPorta>");
        return 2;
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("echo host erro: " + ex);
    return 1;
}

static void EchoLoop(Stream stream)
{
    Span<byte> header = stackalloc byte[4];
    while (ReadExactly(stream, header))
    {
        uint len = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (len > 64 * 1024 * 1024)
        {
            return;
        }

        byte[] body = new byte[len];
        if (!ReadExactly(stream, body))
        {
            return;
        }

        stream.Write(header);
        stream.Write(body);
        stream.Flush();
    }
}

static bool ReadExactly(Stream stream, Span<byte> buffer)
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
