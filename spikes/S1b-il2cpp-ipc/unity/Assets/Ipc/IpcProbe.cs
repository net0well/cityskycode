using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace CitySky.Spike.Ipc
{
    /// <summary>
    /// S1b: do player (IL2CPP), inicia o echo host (.NET 10 self-contained em StreamingAssets),
    /// conecta por named pipe e por TCP loopback, mede ida-e-volta, testa mensagem de 8 MB e a
    /// recuperação quando o host cai. Escreve o resultado num arquivo e encerra.
    /// </summary>
    public static class IpcProbe
    {
        private const int RoundTrips = 1000;
        private static readonly StringBuilder Report = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Run()
        {
            string backend = IsIl2cpp() ? "IL2CPP" : "Mono";
            Line($"runtime=Player-{backend} unity={Application.unityVersion}");

            string hostExe = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "echohost", "CitySky.Spike.EchoHost.exe"));
            Line($"hostExists={File.Exists(hostExe)}");

            try { PipeTest(hostExe); } catch (Exception e) { Line("pipe EXCEPTION " + e.Message); }
            try { TcpTest(hostExe); } catch (Exception e) { Line("tcp EXCEPTION " + e.Message); }
            try { RecoveryTest(hostExe); } catch (Exception e) { Line("recovery EXCEPTION " + e.Message); }

            string dir = Path.GetDirectoryName(Application.dataPath) ?? Application.persistentDataPath;
            File.WriteAllText(Path.Combine(dir, $"s1b-{backend}.txt"), Report.ToString());
            UnityEngine.Debug.Log("S1B_RESULT\n" + Report);
            Application.Quit(0);
        }

        private static void PipeTest(string hostExe)
        {
            string name = "citysky-s1b-" + Guid.NewGuid().ToString("N");
            using var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);
            ChildProcess host = Start(hostExe, $"--pipe {name}");

            if (!WaitForConnection(server, 10000))
            {
                Line("pipe=FAIL (host não conectou)");
                Kill(host);
                return;
            }

            var small = new byte[64];
            double warm = Measure(server, small, 50);
            double rt = Measure(server, small, RoundTrips);

            var big = new byte[8 * 1024 * 1024];
            double bigMs = Measure(server, big, 1);

            Line($"pipe=OK startWarmMs={warm:F2} roundTripMsAvg={rt:F3} msg8MBms={bigMs:F1}");
            server.Close();
            host.WaitForExit(2000);
            Line($"pipe hostExitedOnClose={host.HasExited}");
            Kill(host);
        }

        private static void TcpTest(string hostExe)
        {
            string portFile = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "s1b-port-" + Guid.NewGuid().ToString("N") + ".txt"));
            ChildProcess host = Start(hostExe, $"--tcp \"{portFile}\"");

            int port = 0;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10000 && port == 0)
            {
                if (File.Exists(portFile) && int.TryParse(SafeRead(portFile), out int p) && p > 0)
                {
                    port = p;
                }
                else
                {
                    System.Threading.Thread.Sleep(20);
                }
            }

            if (port == 0)
            {
                Line("tcp=FAIL (sem porta)");
                Kill(host);
                return;
            }

            using var client = new TcpClient();
            client.Connect("127.0.0.1", port);
            client.NoDelay = true;
            NetworkStream stream = client.GetStream();

            var small = new byte[64];
            Measure(stream, small, 50);
            double rt = Measure(stream, small, RoundTrips);
            var big = new byte[8 * 1024 * 1024];
            double bigMs = Measure(stream, big, 1);

            Line($"tcp=OK port={port} roundTripMsAvg={rt:F3} msg8MBms={bigMs:F1}");
            Kill(host);
        }

        private static void RecoveryTest(string hostExe)
        {
            string name = "citysky-s1b-rec-" + Guid.NewGuid().ToString("N");
            var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);
            ChildProcess host = Start(hostExe, $"--pipe {name}");
            if (!WaitForConnection(server, 10000))
            {
                Line("recovery=FAIL (sem conexão inicial)");
                Kill(host);
                return;
            }

            // Mata o host e mede quanto o player leva para detectar a falha.
            Kill(host);
            var sw = Stopwatch.StartNew();
            bool detected = false;
            try
            {
                var buf = new byte[64];
                for (int i = 0; i < 100 && !detected; i++)
                {
                    WriteFrame(server, buf);
                    if (!TryReadFrame(server, buf))
                    {
                        detected = true;
                    }
                }
            }
            catch (Exception)
            {
                detected = true;
            }

            double detectMs = sw.Elapsed.TotalMilliseconds;
            server.Dispose();

            // Reinicia e reconecta.
            var sw2 = Stopwatch.StartNew();
            var server2 = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);
            ChildProcess host2 = Start(hostExe, $"--pipe {name}");
            bool reconnected = WaitForConnection(server2, 10000);
            double restartMs = sw2.Elapsed.TotalMilliseconds;
            Line($"recovery=OK detectedMs={detectMs:F0} detected={detected} reconnected={reconnected} restartMs={restartMs:F0}");
            server2.Dispose();
            Kill(host2);
        }

        private static double Measure(Stream stream, byte[] payload, int count)
        {
            var sw = Stopwatch.StartNew();
            var buf = new byte[payload.Length];
            for (int i = 0; i < count; i++)
            {
                WriteFrame(stream, payload);
                if (!TryReadFrame(stream, buf))
                {
                    throw new IOException("sem resposta");
                }
            }

            return sw.Elapsed.TotalMilliseconds / count;
        }

        private static void WriteFrame(Stream stream, byte[] body)
        {
            var header = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(header, (uint)body.Length);
            stream.Write(header, 0, 4);
            stream.Write(body, 0, body.Length);
            stream.Flush();
        }

        private static bool TryReadFrame(Stream stream, byte[] buffer)
        {
            var header = new byte[4];
            if (!ReadExactly(stream, header, 4))
            {
                return false;
            }

            uint len = BinaryPrimitives.ReadUInt32LittleEndian(header);
            return ReadExactly(stream, buffer, (int)len);
        }

        private static bool ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = stream.Read(buffer, read, count - read);
                if (n == 0)
                {
                    return false;
                }

                read += n;
            }

            return true;
        }

        private static bool WaitForConnection(NamedPipeServerStream server, int timeoutMs)
        {
            bool connected = false;
            var t = new System.Threading.Thread(() =>
            {
                try { server.WaitForConnection(); connected = true; } catch { }
            });
            t.IsBackground = true;
            t.Start();
            if (!t.Join(timeoutMs))
            {
                try { server.Dispose(); } catch { }
                t.Join(1000);
            }

            return connected;
        }

        private static ChildProcess Start(string exe, string args)
        {
            // No IL2CPP, System.Diagnostics.Process.Start falha nos dois modos (UseShellExecute
            // false e true): a exceção reportada é só "Success". O caminho de produção
            // (ISandboxLauncher, igual ao S6) lança por CreateProcess nativo, que funciona.
            return ChildProcess.Start(exe, args);
        }

        private static void Kill(ChildProcess p)
        {
            try { p.Kill(); } finally { p.Dispose(); }
        }

        private static string SafeRead(string path)
        {
            try { return File.ReadAllText(path); } catch { return string.Empty; }
        }

        private static void Line(string s) => Report.AppendLine(s);

        private static bool IsIl2cpp()
        {
#if ENABLE_IL2CPP
            return true;
#else
            return false;
#endif
        }
    }
}
