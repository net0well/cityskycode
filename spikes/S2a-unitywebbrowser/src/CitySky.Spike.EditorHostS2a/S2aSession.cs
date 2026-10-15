using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using Photino.NET;

namespace CitySky.Spike.EditorHost;

/// <summary>
/// Spike S2a — opção (B'): o host cria o editor (PhotinoX/WebView2) e o reparenta como janela-filha
/// (WS_CHILD) do player Unity real, ocupando o painel lateral direito. Mede, de forma objetiva:
/// se o editor compõe por cima do swapchain da Unity (amostragem de pixels do resultado do DWM),
/// se os estados do painel batem com o retângulo esperado, digitação com acentos e memória.
/// Os itens subjetivos (flicker percebido, IME, nitidez em DPI, imersão) ficam no checklist humano.
/// </summary>
internal sealed class S2aSession
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _unityExe;
    private readonly string _outputPath;
    private readonly string _shotDir;
    private readonly bool _hold;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _waiters = new();
    private readonly ConcurrentQueue<string> _pageErrors = new();
    private readonly JsonObject _results = new();
    private readonly string _webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    private PhotinoWindow _window = null!;
    private UnityGameWindow? _game;
    private string _panelState = "normal";

    private const int HudHeight = 40;
    private const int CollapsedWidth = 44;
    private const double NormalFraction = 0.34;
    private const double ExpandedFraction = 0.70;

    public S2aSession(string unityExe, string outputPath, string shotDir, bool hold)
    {
        _unityExe = unityExe;
        _outputPath = outputPath;
        _shotDir = shotDir;
        _hold = hold;
    }

    public int Run()
    {
        _window = new PhotinoWindow()
            .SetLogVerbosity(0)
            .SetTitle("City: SkyCode — editor (spike S2a)")
            .SetUseOsDefaultSize(false)
            .SetSize(900, 700)
            .SetUseOsDefaultLocation(false)
            .SetLocation(new Point(200, 140))
            .SetDevToolsEnabled(false)
            .SetContextMenuEnabled(false)
            .SetUserDataFolder(Path.Combine(Path.GetTempPath(), "CitySkySpikeS2a"))
            .RegisterCustomSchemeHandler("app", ServeFile)
            .RegisterNavigationStartingHandler((_, e) => { if (e.Uri.Scheme != "app") { e.Cancel = true; } })
            .RegisterWebMessageReceivedHandler((_, e) => OnMessage(e.Message))
            .Load("app://localhost/index.html");

        int code = new PhotinoApplication().Run(_window);
        _game?.Dispose();
        return code;
    }

    private Stream? ServeFile(object sender, string scheme, string url, out string contentType)
    {
        string relative = Uri.UnescapeDataString(new Uri(url).AbsolutePath).TrimStart('/');
        string fullPath = Path.GetFullPath(Path.Combine(_webRoot, relative));
        contentType = Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".js" => "text/javascript",
            ".css" => "text/css",
            ".ttf" => "font/ttf",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".svg" => "image/svg+xml",
            ".json" => "application/json",
            _ => "application/octet-stream",
        };
        if (!fullPath.StartsWith(_webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            _pageErrors.Enqueue("404 " + url);
            return null;
        }

        return File.OpenRead(fullPath);
    }

    private void OnMessage(string raw)
    {
        using JsonDocument document = JsonDocument.Parse(raw);
        JsonElement root = document.RootElement;
        string type = root.GetProperty("type").GetString() ?? string.Empty;
        int id = root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt32() : 0;
        JsonElement payload = root.TryGetProperty("payload", out JsonElement p) ? p.Clone() : default;

        switch (type)
        {
            case "ready":
                _ = Task.Run(RunTestAsync);
                break;
            case "completion":
                Send(new { type = "completionResult", id, payload = new { items = Array.Empty<object>(), serverMs = 0 } });
                break;
            case "diagnostics":
                Send(new { type = "diagnosticsResult", id, payload = new { items = Array.Empty<object>(), serverMs = 0 } });
                break;
            case "error":
            case "cspViolation":
                _pageErrors.Enqueue($"{type}: {payload}");
                break;
            default:
                if (_waiters.TryRemove(type, out TaskCompletionSource<JsonElement>? waiter))
                {
                    waiter.TrySetResult(payload);
                }

                break;
        }
    }

    private void Send(object message)
    {
        string json = JsonSerializer.Serialize(message, Json);
        _window.Invoke(() => _window.SendWebMessage(json));
    }

    private async Task<JsonElement> RequestAsync(string type, object? payload, string replyType, TimeSpan timeout)
    {
        var waiter = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiters[replyType] = waiter;
        Send(new { type, payload });
        return await waiter.Task.WaitAsync(timeout);
    }

    // ------------------------------------------------------------------------------------------
    private async Task RunTestAsync()
    {
        try
        {
            _results["unityExe"] = _unityExe;
            _game = UnityGameWindow.Start(_unityExe, TimeSpan.FromSeconds(30));
            IntPtr editor = _window.WindowHandle;
            IntPtr game = _game.Handle;
            _results["unityWindowClass"] = Native.ClassName(game);

            // O jogo recorta filhos ao desenhar; embute o editor como filho.
            long gameStyle = Native.GetWindowLongPtrW(game, Native.GwlStyle).ToInt64() | Native.WsClipChildren;
            Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(gameStyle));

            long start = Stopwatch.GetTimestamp();
            EmbedAsChild(editor, game);
            _results["embedMs"] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            Thread.Sleep(500);
            _results["isChildOfGame"] = Native.IsChild(game, editor);
            _results["editorVisible"] = Native.IsWindowVisible(editor);

            // Conteúdo de demonstração (código + console) para o editor ter algo desenhado.
            await RequestAsync("demo", null, "demoReady", TimeSpan.FromSeconds(10));
            Thread.Sleep(1500);

            Directory.CreateDirectory(_shotDir);

            // Estados do painel: para cada um, verifica o retângulo e a composição por cima do swapchain.
            var states = new JsonObject();
            foreach (string state in new[] { "normal", "expanded", "minimized" })
            {
                await RequestAsync("setState", state, "stateApplied", TimeSpan.FromSeconds(10));
                _panelState = state;
                PlaceInRightPanel(editor, game);
                Native.SetForegroundWindow(game);
                Thread.Sleep(700);

                var entry = new JsonObject
                {
                    ["panelMatchesExpected"] = PanelMatches(editor, game),
                    ["editorVisible"] = Native.IsWindowVisible(editor),
                };
                if (OperatingSystem.IsWindows())
                {
                    entry["composition"] = CompositionProbe(editor, game, frames: 8);
                    SaveShot(game, Path.Combine(_shotDir, "s2a-" + state + ".png"));
                }

                states[state] = entry;
            }

            _results["states"] = states;

            // Volta ao normal para os testes restantes.
            await RequestAsync("setState", "normal", "stateApplied", TimeSpan.FromSeconds(10));
            _panelState = "normal";
            PlaceInRightPanel(editor, game);
            Thread.Sleep(400);

            _results["typing"] = await TypingTestAsync(editor, game);
            _results["resize"] = ResizeTest(editor, game);
            if (OperatingSystem.IsWindows())
            {
                _results["fullscreenBorderless"] = await FullscreenTestAsync(editor, game, gameStyle);
            }

            _results["memory"] = MemorySnapshot();
            _results["pingGameMs"] = Native.Ping(game);
            _results["pingEditorMs"] = Native.Ping(editor);
        }
        catch (Exception ex)
        {
            _results["fatal"] = ex.ToString();
        }

        _results["pageErrors"] = new JsonArray(_pageErrors.Select(e => (JsonNode)e).ToArray());
        WriteResults();
        Console.WriteLine(_results.ToJsonString(Json));

        if (_hold)
        {
            Console.WriteLine("S2A_HOLD: janela aberta para inspeção manual (feche o player Unity para encerrar).");
            return; // não fecha: checklist humano
        }

        Finish();
    }

    // ------------------------------------------------------------------------------------------
    // Composição: captura o que o DWM mostra (igual ao que o humano vê) e classifica os pixels do
    // painel. Editor = UI escura (luminância baixa); cena Unity = clara (céu/chão/cubos). Repete em
    // vários frames para flagrar flicker (a cena orbita sempre, então frames variando = pintou por cima).
    // ------------------------------------------------------------------------------------------
    [SupportedOSPlatform("windows")]
    private JsonObject CompositionProbe(IntPtr editor, IntPtr game, int frames)
    {
        Native.GetWindowRect(editor, out Native.Rect panel);
        Native.GetWindowRect(game, out Native.Rect g);
        // Região de controle: faixa à esquerda do painel, dentro do jogo (deve mostrar a cena).
        var control = new Native.Rect { Left = g.Left + 10, Top = g.Top + HudHeight + 10, Right = panel.Left - 10, Bottom = g.Bottom - 10 };

        var panelDark = new List<double>();
        var controlDark = new List<double>();
        for (int i = 0; i < frames; i++)
        {
            panelDark.Add(DarkFraction(panel));
            if (control.Right - control.Left > 20 && control.Bottom - control.Top > 20)
            {
                controlDark.Add(DarkFraction(control));
            }

            Thread.Sleep(110);
        }

        double min = panelDark.Min();
        double max = panelDark.Max();
        double mean = panelDark.Average();
        // Veredito objetivo: painel consistentemente escuro (editor compôs por cima) e controle claro (cena).
        bool composedOk = min >= 0.70 && (controlDark.Count == 0 || controlDark.Average() <= 0.35);
        bool flicker = (max - min) >= 0.25;
        return new JsonObject
        {
            ["panelDarkMin"] = Math.Round(min, 3),
            ["panelDarkMean"] = Math.Round(mean, 3),
            ["panelDarkMax"] = Math.Round(max, 3),
            ["controlDarkMean"] = controlDark.Count == 0 ? -1 : Math.Round(controlDark.Average(), 3),
            ["editorComposedOverScene"] = composedOk,
            ["flickerSuspected"] = flicker,
            ["verdict"] = composedOk && !flicker ? "OK: editor compõe estável sobre o swapchain"
                        : composedOk ? "ATENÇÃO: compõe mas varia entre frames (possível flicker) — conferir no checklist"
                        : "FALHA objetiva: a cena aparece na região do painel — conferir no checklist",
        };
    }

    [SupportedOSPlatform("windows")]
    private static double DarkFraction(Native.Rect r)
    {
        int w = r.Right - r.Left;
        int h = r.Bottom - r.Top;
        if (w <= 2 || h <= 2)
        {
            return 0;
        }

        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (Graphics gfx = Graphics.FromImage(bmp))
        {
            gfx.CopyFromScreen(r.Left, r.Top, 0, 0, bmp.Size);
        }

        int step = Math.Max(4, Math.Min(w, h) / 40);
        int dark = 0;
        int total = 0;
        for (int y = 0; y < h; y += step)
        {
            for (int x = 0; x < w; x += step)
            {
                Color c = bmp.GetPixel(x, y);
                double lum = 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
                if (lum < 80)
                {
                    dark++;
                }

                total++;
            }
        }

        return total == 0 ? 0 : (double)dark / total;
    }

    [SupportedOSPlatform("windows")]
    private static void SaveShot(IntPtr window, string path)
    {
        Native.GetWindowRect(window, out Native.Rect r);
        int w = r.Right - r.Left;
        int h = r.Bottom - r.Top;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (Graphics gfx = Graphics.FromImage(bmp))
        {
            gfx.CopyFromScreen(r.Left, r.Top, 0, 0, bmp.Size);
        }

        bmp.Save(path, ImageFormat.Png);
    }

    // ------------------------------------------------------------------------------------------
    private void EmbedAsChild(IntPtr editor, IntPtr game)
    {
        _window.Invoke(() =>
        {
            Native.SetWindowLongPtrW(editor, Native.GwlpHwndParent, IntPtr.Zero);
            long style = Native.GetWindowLongPtrW(editor, Native.GwlStyle).ToInt64();
            style &= ~(Native.WsPopup | Native.WsCaption | Native.WsThickFrame | Native.WsSysMenu | Native.WsMinimizeBox | Native.WsMaximizeBox);
            style |= Native.WsChild | Native.WsClipSiblings;
            Native.SetWindowLongPtrW(editor, Native.GwlStyle, new IntPtr(style));
            Native.SetParent(editor, game);
        });
        PlaceInRightPanel(editor, game);
    }

    private Native.Rect PanelRect(Native.Rect client)
    {
        int width = _panelState switch
        {
            "minimized" => CollapsedWidth,
            "expanded" => (int)(client.Right * ExpandedFraction),
            _ => (int)(client.Right * NormalFraction),
        };
        return new Native.Rect { Left = client.Right - width, Top = HudHeight, Right = client.Right, Bottom = client.Bottom };
    }

    private void PlaceInRightPanel(IntPtr editor, IntPtr game)
    {
        Native.GetClientRect(game, out Native.Rect client);
        Native.Rect p = PanelRect(client);
        _window.Invoke(() => Native.SetWindowPos(
            editor, IntPtr.Zero, p.Left, p.Top, p.Right - p.Left, p.Bottom - p.Top,
            Native.SwpNoZOrder | Native.SwpFrameChanged | Native.SwpShowWindow));
    }

    private bool PanelMatches(IntPtr editor, IntPtr game)
    {
        Native.GetClientRect(game, out Native.Rect client);
        Native.Rect p = PanelRect(client);
        Native.GetWindowRect(editor, out Native.Rect e);
        Native.MapWindowPoints(IntPtr.Zero, game, ref e, 2);
        return Math.Abs(e.Left - p.Left) <= 2 && Math.Abs(e.Top - p.Top) <= 2
            && Math.Abs(e.Right - p.Right) <= 2 && Math.Abs(e.Bottom - p.Bottom) <= 2;
    }

    private JsonObject ResizeTest(IntPtr editor, IntPtr game)
    {
        Native.GetWindowRect(game, out Native.Rect g);
        Native.SetWindowPos(game, IntPtr.Zero, g.Left, g.Top, g.Right - g.Left + 160, g.Bottom - g.Top + 90, Native.SwpNoZOrder | Native.SwpNoActivate);
        Thread.Sleep(300);
        bool matchedBefore = PanelMatches(editor, game);
        PlaceInRightPanel(editor, game);
        Thread.Sleep(200);
        bool matchedAfter = PanelMatches(editor, game);
        // volta ao tamanho original
        Native.SetWindowPos(game, IntPtr.Zero, g.Left, g.Top, g.Right - g.Left, g.Bottom - g.Top, Native.SwpNoZOrder | Native.SwpNoActivate);
        Thread.Sleep(200);
        PlaceInRightPanel(editor, game);
        return new JsonObject { ["panelMatchedBeforeReposition"] = matchedBefore, ["panelMatchesAfterReposition"] = matchedAfter };
    }

    [SupportedOSPlatform("windows")]
    private async Task<JsonObject> FullscreenTestAsync(IntPtr editor, IntPtr game, long windowedStyle)
    {
        Native.GetWindowRect(game, out Native.Rect windowed);
        long borderless = windowedStyle & ~(Native.WsCaption | Native.WsThickFrame | Native.WsSysMenu | Native.WsMinimizeBox | Native.WsMaximizeBox);
        Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(borderless));
        int sw = Native.GetSystemMetrics(Native.SmCxScreen);
        int sh = Native.GetSystemMetrics(Native.SmCyScreen);
        Native.SetWindowPos(game, IntPtr.Zero, 0, 0, sw, sh, Native.SwpNoZOrder | Native.SwpFrameChanged | Native.SwpShowWindow);
        Thread.Sleep(600);
        PlaceInRightPanel(editor, game);
        Native.SetForegroundWindow(game);
        Thread.Sleep(500);

        var result = new JsonObject
        {
            ["screen"] = $"{sw}x{sh}",
            ["isChildOfGame"] = Native.IsChild(game, editor),
            ["panelMatchesExpected"] = PanelMatches(editor, game),
            ["composition"] = CompositionProbe(editor, game, frames: 6),
        };
        SaveShot(game, Path.Combine(_shotDir, "s2a-fullscreen.png"));

        // De volta para janela.
        Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(windowedStyle));
        Native.SetWindowPos(game, IntPtr.Zero, windowed.Left, windowed.Top, windowed.Right - windowed.Left, windowed.Bottom - windowed.Top, Native.SwpNoZOrder | Native.SwpFrameChanged);
        Thread.Sleep(500);
        PlaceInRightPanel(editor, game);
        result["backToWindowedVisible"] = Native.IsWindowVisible(editor);
        result["backToWindowedPanelMatches"] = PanelMatches(editor, game);
        _ = await Task.FromResult(0);
        return result;
    }

    // ------------------------------------------------------------------------------------------
    // Digitação com acentos na janela-filha. Como filho, o foco não entra sozinho no WebView2;
    // tenta estratégias e usa a primeira que funcionar (igual ao S2b).
    // ------------------------------------------------------------------------------------------
    private async Task<JsonObject> TypingTestAsync(IntPtr editor, IntPtr game)
    {
        var result = new JsonObject { ["foregroundGranted"] = Native.SetForegroundWindow(game) };
        Thread.Sleep(300);
        if (!Native.IsForeground(game))
        {
            result["skipped"] = true;
            result["reason"] = "o player Unity não ficou em primeiro plano (regra de foreground do Windows); digitação fica no checklist manual";
            return result;
        }

        string? working = null;
        var probes = new JsonObject();
        foreach (string strategy in new[] { "setFocusPanel", "sendActivate", "setFocusChromium", "guardedClick" })
        {
            bool applied = FocusEmbedded(strategy, editor, game);
            await RequestAsync("prepareTyping", null, "typingReady", TimeSpan.FromSeconds(10));
            Thread.Sleep(300);
            string probe = Repeat("ação ", 30);
            int sent = applied ? TypeGuarded(game, probe, 20) : 0;
            Thread.Sleep(500);
            string got = (await RequestAsync("getValue", null, "value", TimeSpan.FromSeconds(10))).GetString() ?? string.Empty;
            probes[strategy] = new JsonObject { ["applied"] = applied, ["sent"] = sent, ["received"] = got.Length };
            if (got.Length > 0 && got == probe[..sent])
            {
                working = strategy;
                break;
            }
        }

        result["focusProbes"] = probes;
        result["workingFocusStrategy"] = working;
        if (working is null)
        {
            result["note"] = "nenhuma estratégia de foco automática funcionou; digitação real fica no checklist manual";
            return result;
        }

        FocusEmbedded(working, editor, game);
        await RequestAsync("prepareTyping", null, "typingReady", TimeSpan.FromSeconds(10));
        Thread.Sleep(300);
        string text = Repeat("city.Log(\"Ação, coração, pão, você, à, útil, ç, õ, é!\"); ", 20);
        int typed = TypeGuarded(game, text, 8);
        Thread.Sleep(800);
        string actual = (await RequestAsync("getValue", null, "value", TimeSpan.FromSeconds(10))).GetString() ?? string.Empty;
        string expected = text[..typed];
        result["charsSent"] = typed;
        result["identical"] = actual == expected;
        result["receivedLength"] = actual.Length;
        result["firstMismatchIndex"] = FirstMismatch(expected, actual);
        return result;
    }

    private bool FocusEmbedded(string strategy, IntPtr editor, IntPtr game)
    {
        switch (strategy)
        {
            case "setFocusPanel":
                _window.Invoke(() => Native.SetFocus(editor));
                return true;
            case "sendActivate":
                _window.Invoke(() =>
                {
                    Native.SendMessageTimeoutW(editor, Native.WmActivate, new IntPtr(Native.WaActive), IntPtr.Zero, Native.SmtoAbortIfHung, 1000, out _);
                    Native.SetFocus(editor);
                });
                return true;
            case "setFocusChromium":
                IntPtr chromium = Native.FindChromiumInputWindow(editor);
                if (chromium == IntPtr.Zero)
                {
                    return false;
                }

                _window.Invoke(() => Native.SetFocus(chromium));
                return true;
            case "guardedClick":
                Native.GetWindowRect(editor, out Native.Rect r);
                var point = new Native.Point { X = (r.Left + r.Right) / 2, Y = (r.Top + r.Bottom) / 2 };
                IntPtr hit = Native.WindowFromPoint(point);
                if (Native.GetAncestor(hit, Native.GaRoot) != game || (hit != editor && !Native.IsChild(editor, hit)))
                {
                    return false;
                }

                Native.GetCursorPos(out Native.Point original);
                Native.SetCursorPos(point.X, point.Y);
                var clicks = new[]
                {
                    new Native.MouseInputUnion { Type = Native.InputMouse, Mouse = new Native.MouseInput { Flags = Native.MouseeventfLeftDown } },
                    new Native.MouseInputUnion { Type = Native.InputMouse, Mouse = new Native.MouseInput { Flags = Native.MouseeventfLeftUp } },
                };
                Native.SendMouseInput(2, clicks, System.Runtime.InteropServices.Marshal.SizeOf<Native.MouseInputUnion>());
                Thread.Sleep(200);
                Native.SetCursorPos(original.X, original.Y);
                return true;
            default:
                return false;
        }
    }

    private static int TypeGuarded(IntPtr topLevel, string text, int delayMs)
    {
        var inputs = new Native.Input[2];
        for (int i = 0; i < text.Length; i++)
        {
            if (!Native.IsForeground(topLevel))
            {
                return i;
            }

            inputs[0] = new Native.Input { Type = Native.InputKeyboard, Keyboard = new Native.KeyboardInput { Scan = text[i], Flags = Native.KeyeventfUnicode } };
            inputs[1] = new Native.Input { Type = Native.InputKeyboard, Keyboard = new Native.KeyboardInput { Scan = text[i], Flags = Native.KeyeventfUnicode | Native.KeyeventfKeyUp } };
            Native.SendInput(2, inputs, System.Runtime.InteropServices.Marshal.SizeOf<Native.Input>());
            Thread.Sleep(delayMs);
        }

        return text.Length;
    }

    private JsonObject MemorySnapshot()
    {
        IReadOnlyList<ProcessMemory> processes = ProcessTree.Measure(Environment.ProcessId, _game!.ProcessId);
        double MB(long bytes) => Math.Round(bytes / (1024.0 * 1024.0), 1);
        return new JsonObject
        {
            ["hostPrivateMB"] = MB(processes.Where(p => p.Pid == Environment.ProcessId).Sum(p => p.PrivateBytes)),
            ["webView2PrivateMB"] = MB(processes.Where(p => p.Pid != Environment.ProcessId).Sum(p => p.PrivateBytes)),
            ["totalPrivateMB"] = MB(processes.Sum(p => p.PrivateBytes)),
            ["webView2Version"] = ProcessTree.WebView2Version(processes),
            ["processes"] = new JsonArray(processes
                .Select(p => (JsonNode)new JsonObject { ["name"] = p.Name, ["privateMB"] = MB(p.PrivateBytes) })
                .ToArray()),
        };
    }

    private void WriteResults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_outputPath)!);
        File.WriteAllText(_outputPath, _results.ToJsonString(Json));
    }

    private void Finish()
    {
        try { _window.Invoke(() => _window.Close()); } catch { /* já fechada */ }
        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            Environment.Exit(0);
        });
    }

    private static string Repeat(string unit, int count)
    {
        var builder = new System.Text.StringBuilder(unit.Length * count);
        for (int i = 0; i < count; i++)
        {
            builder.Append(unit);
        }

        return builder.ToString();
    }

    private static int FirstMismatch(string expected, string actual)
    {
        int n = Math.Min(expected.Length, actual.Length);
        for (int i = 0; i < n; i++)
        {
            if (expected[i] != actual[i])
            {
                return i;
            }
        }

        return expected.Length == actual.Length ? -1 : n;
    }
}
