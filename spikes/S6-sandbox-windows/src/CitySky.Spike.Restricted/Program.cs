using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;

// Processo restrito do S6. Recebe o nome de uma ação e tenta executá-la, reportando PERMITIU/NEGOU.
// O objetivo é verificar que a camada L5 (Job Object/AppContainer) BLOQUEIA cada tentativa.
// Saída: uma linha "ACTION=<nome> RESULT=<ALLOWED|DENIED> DETAIL=<...>".
string action = args.Length > 0 ? args[0] : "ping";
string allowedDir = args.Length > 1 ? args[1] : AppContext.BaseDirectory;

try
{
    switch (action)
    {
        case "ping":
            Report("ALLOWED", "processo iniciou");
            break;

        case "read-userfile":
        {
            // Tenta ler um arquivo na pasta de documentos do usuário (fora da pasta permitida).
            string target = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "s6-canario.txt");
            string content = File.ReadAllText(target);
            Report("ALLOWED", $"leu {content.Length} chars de {target}");
            break;
        }

        case "write-userfile":
        {
            string target = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "s6-saida.txt");
            File.WriteAllText(target, "o processo restrito escreveu aqui");
            Report("ALLOWED", $"escreveu em {target}");
            break;
        }

        case "write-tempfile":
        {
            string target = Path.Combine(Path.GetTempPath(), "s6-temp.txt");
            File.WriteAllText(target, "temp");
            Report("ALLOWED", $"escreveu em {target}");
            break;
        }

        case "read-allowed":
        {
            // Leitura DENTRO da pasta permitida: deve funcionar (a sandbox não pode quebrar o básico).
            string target = Path.Combine(allowedDir, "CitySky.Spike.Restricted.dll");
            long size = new FileInfo(target).Length;
            Report("ALLOWED", $"leu o próprio assembly ({size} bytes)");
            break;
        }

        case "http":
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            string body = client.GetStringAsync("http://example.com/").GetAwaiter().GetResult();
            Report("ALLOWED", $"HTTP externo devolveu {body.Length} chars");
            break;
        }

        case "tcp-loopback":
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("127.0.0.1", 9); // porta discard; interessa se a conexão é permitida
            Report("ALLOWED", "conectou em 127.0.0.1:9");
            break;
        }

        case "start-process":
        {
            using Process? p = Process.Start(new ProcessStartInfo("cmd.exe", "/c echo oi") { UseShellExecute = false });
            Report("ALLOWED", $"iniciou processo filho PID {p?.Id}");
            break;
        }

        case "memory":
        {
            // Aloca blocos de 32 MB até o limite do Job encerrar o processo.
            var blocks = new List<byte[]>();
            for (int i = 0; i < 64; i++)
            {
                var block = new byte[32 * 1024 * 1024];
                block[0] = 1;
                block[^1] = 1;
                blocks.Add(block);
            }

            Report("ALLOWED", $"alocou {blocks.Count * 32} MB sem ser encerrado");
            break;
        }

        case "cpu":
        {
            // Trabalho puro de CPU por ~3 s; o launcher mede o tempo de CPU consumido (limite de taxa).
            var sw = Stopwatch.StartNew();
            double x = 0;
            while (sw.ElapsedMilliseconds < 3000)
            {
                x += Math.Sqrt(x + 1);
            }

            Report("ALLOWED", $"rodou CPU por 3 s (x={x:F0})");
            break;
        }

        default:
            Report("DENIED", "ação desconhecida");
            break;
    }

    return 0;
}
catch (Exception ex)
{
    Report("DENIED", $"{ex.GetType().Name}: {ex.Message}");
    return 1;
}

void Report(string result, string detail) =>
    Console.WriteLine($"ACTION={action} RESULT={result} DETAIL={detail}");
