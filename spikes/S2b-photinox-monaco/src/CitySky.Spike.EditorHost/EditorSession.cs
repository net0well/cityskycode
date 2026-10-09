using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using Photino.NET;

namespace CitySky.Spike.EditorHost;

/// <summary>Janela PhotinoX + ponte de mensagens + (opcional) autoteste do spike S2b.</summary>
internal sealed class EditorSession
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly bool _selfTest;
    private readonly string? _screenshotDir;
    private readonly string _outputPath;
    private readonly Stopwatch _sinceStart;
    private readonly RoslynService _roslyn = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _waiters = new();
    private readonly ConcurrentQueue<string> _pageErrors = new();
    private readonly JsonObject _results = new();
    private readonly string _webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    private PhotinoWindow _window = null!;
    private FakeGameWindow? _game;
    private double _windowCreatedMs;
    private string _lastHitTest = string.Empty;
    private string _panelState = "normal";
    private bool _embedded;

    public EditorSession(bool selfTest, string? screenshotDir, string outputPath, Stopwatch sinceStart)
    {
        _selfTest = selfTest;
        _screenshotDir = screenshotDir;
        _outputPath = outputPath;
        _sinceStart = sinceStart;
    }

    public int Run()
    {
        if (_selfTest || _screenshotDir is not null)
        {
            _game = FakeGameWindow.Start(TimeSpan.FromSeconds(15));
        }

        WarmUpRoslyn();

        _windowCreatedMs = _sinceStart.Elapsed.TotalMilliseconds;
        _window = new PhotinoWindow()
            .SetLogVerbosity(0)
            .SetTitle("City: SkyCode — editor (spike S2b)")
            .SetUseOsDefaultSize(false)
            .SetSize(900, 700)
            .SetUseOsDefaultLocation(false)
            .SetLocation(new Point(220, 160))
            .SetDevToolsEnabled(!_selfTest && _screenshotDir is null)
            .SetContextMenuEnabled(false)
            .SetUserDataFolder(Path.Combine(Path.GetTempPath(), "CitySkySpikeS2b"))
            .RegisterCustomSchemeHandler("app", ServeFile)
            .RegisterNavigationStartingHandler((_, e) =>
            {
                if (e.Uri.Scheme != "app")
                {
                    e.Cancel = true; // navegação externa bloqueada
                }
            })
            // O PhotinoX nunca cria pop-ups controlados pelo navegador; só registramos a tentativa.
            .RegisterNewWindowRequestedHandler((_, e) => _pageErrors.Enqueue("popup bloqueado: " + e.Uri))
            .RegisterWebMessageReceivedHandler((_, e) => OnMessage(e.Message))
            .Load("app://localhost/index.html");

        if (_selfTest)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(40));
                if (!_waiters.ContainsKey("ready-seen"))
                {
                    _results["fatal"] = "A página não enviou 'ready' em 40 s. Erros: " + string.Join(" | ", _pageErrors);
                    Finish();
                }
            });
        }

        int code = new PhotinoApplication().Run(_window);
        _game?.Dispose();
        return code;
    }

    private void WarmUpRoslyn()
    {
        const string text = "using CitySky;\nvar city = CityGame.Current;\ncity.";
        long start = Stopwatch.GetTimestamp();
        _roslyn.GetCompletionsAsync(text, text.Length).GetAwaiter().GetResult();
        double completionMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        start = Stopwatch.GetTimestamp();
        _roslyn.GetDiagnosticsAsync(text).GetAwaiter().GetResult();
        double diagnosticsMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        _results["roslynColdStart"] = new JsonObject { ["firstCompletionMs"] = completionMs, ["firstDiagnosticsMs"] = diagnosticsMs };
    }

    // ------------------------------------------------------------------------------------------
    // Esquema app:// — serve só arquivos de wwwroot (sem sair da pasta).
    // ------------------------------------------------------------------------------------------
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

    // ------------------------------------------------------------------------------------------
    // Ponte
    // ------------------------------------------------------------------------------------------
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
                _results["pageReadyMsSinceStart"] = _sinceStart.Elapsed.TotalMilliseconds;
                _results["pageReadyMsSinceWindowCreated"] = _sinceStart.Elapsed.TotalMilliseconds - _windowCreatedMs;
                _results["pageHref"] = payload.GetProperty("href").GetString();
                _waiters.TryAdd("ready-seen", new TaskCompletionSource<JsonElement>());
                if (_selfTest)
                {
                    _ = Task.Run(SelfTestAsync);
                }
                else if (_screenshotDir is not null)
                {
                    _ = Task.Run(ScreenshotsAsync);
                }
                break;

            case "panelState":
                // O aluno clicou em minimizar/expandir dentro do painel: o host redimensiona a janela-filha.
                _panelState = payload.GetString() ?? "normal";
                if (_embedded)
                {
                    PlaceInRightPanel(_window.WindowHandle, _game!.Handle);
                }
                break;

            case "completion":
                string completionText = payload.GetProperty("text").GetString()!;
                int offset = payload.GetProperty("offset").GetInt32();
                _ = Task.Run(async () =>
                {
                    var (items, serverMs) = await _roslyn.GetCompletionsAsync(completionText, offset);
                    Send(new { type = "completionResult", id, payload = new { items, serverMs } });
                });
                break;

            case "diagnostics":
                string diagnosticsText = payload.GetProperty("text").GetString()!;
                _ = Task.Run(async () =>
                {
                    var (items, serverMs) = await _roslyn.GetDiagnosticsAsync(diagnosticsText);
                    Send(new { type = "diagnosticsResult", id, payload = new { items, serverMs } });
                });
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

    private Task<JsonElement> RequestAsync(string type, string replyType, TimeSpan timeout) =>
        RequestAsync(type, payload: null, replyType, timeout);

    private async Task<JsonElement> RequestAsync(string type, object? payload, string replyType, TimeSpan timeout)
    {
        var waiter = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiters[replyType] = waiter;
        Send(new { type, payload });
        return await waiter.Task.WaitAsync(timeout);
    }

    // ------------------------------------------------------------------------------------------
    // Autoteste
    // ------------------------------------------------------------------------------------------
    private async Task SelfTestAsync()
    {
        try
        {
            JsonElement web = await RequestAsync("runSelfTest", "selfTestResult", TimeSpan.FromMinutes(3));
            _results["web"] = JsonNode.Parse(web.GetRawText());
            _results["owner"] = OwnerTest();                  // janela separada com dono (descartada pelo dono do produto)
            _results["typing"] = await TypingTestAsync();
            _results["embedded"] = await EmbeddedTestAsync(); // janela-filha dentro do jogo, painel direito
            _results["memory"] = MemorySnapshot();

            // A página tenta navegar para fora; se a navegação não for barrada, ela some e o pedido expira.
            try
            {
                await RequestAsync("tryNavigate", "navigationResult", TimeSpan.FromSeconds(4));
                _results["externalNavigationBlocked"] = true;
            }
            catch (TimeoutException)
            {
                _results["externalNavigationBlocked"] = false;
            }
        }
        catch (Exception ex)
        {
            _results["fatal"] = ex.ToString();
        }

        _results["pageErrors"] = new JsonArray(_pageErrors.Select(e => (JsonNode)e).ToArray());
        WriteResults();

        // Por último: fechar o dono e ver o que acontece com a janela do editor.
        try
        {
            _results["ownerClose"] = OwnerCloseTest();
            WriteResults();
        }
        finally
        {
            Finish();
        }
    }

    private JsonObject OwnerTest()
    {
        IntPtr editor = _window.WindowHandle;
        IntPtr game = _game!.Handle;
        var result = new JsonObject();

        Native.GetWindowRect(game, out Native.Rect r);
        int width = 520;
        _window.Invoke(() =>
        {
            _window.SetLocation(new Point(r.Right - width - 20, r.Top + 40));
            _window.SetSize(width, r.Bottom - r.Top - 80);
        });

        _window.Invoke(() => Native.SetWindowLongPtrW(editor, Native.GwlpHwndParent, game));
        result["ownerSet"] = Native.GetWindowLongPtrW(editor, Native.GwlpHwndParent) == game;

        result["gameForegroundGranted"] = Native.SetForegroundWindow(game);
        Thread.Sleep(500);
        result["editorAboveGame"] = Native.IsAbove(editor, game);
        result["pingGameMs"] = Native.Ping(game);
        result["pingEditorMs"] = Native.Ping(editor);

        Native.ShowWindow(game, Native.SwMinimize);
        Thread.Sleep(800);
        result["editorVisibleWhileOwnerMinimized"] = Native.IsWindowVisible(editor);

        Native.ShowWindow(game, Native.SwRestore);
        Thread.Sleep(800);
        result["editorVisibleAfterRestore"] = Native.IsWindowVisible(editor);
        result["editorAboveGameAfterRestore"] = Native.IsAbove(editor, game);

        Native.GetWindowRect(editor, out Native.Rect before);
        Native.GetWindowRect(game, out Native.Rect g);
        Native.SetWindowPos(game, IntPtr.Zero, g.Left + 150, g.Top + 60, 0, 0, Native.SwpNoSize | Native.SwpNoZOrder | Native.SwpNoActivate);
        Thread.Sleep(500);
        Native.GetWindowRect(editor, out Native.Rect after);
        result["editorFollowedOwnerMove"] = before.Left != after.Left || before.Top != after.Top;

        result["pingGameAfterMs"] = Native.Ping(game);
        result["pingEditorAfterMs"] = Native.Ping(editor);
        return result;
    }

    private async Task<JsonObject> TypingTestAsync()
    {
        IntPtr editor = _window.WindowHandle;
        var result = new JsonObject();
        await RequestAsync("prepareTyping", "typingReady", TimeSpan.FromSeconds(10));

        result["foregroundGranted"] = Native.SetForegroundWindow(editor);
        Thread.Sleep(400);
        if (!Native.IsForeground(editor))
        {
            result["skipped"] = true;
            result["reason"] = "O Windows não deu o foco à janela (regra de foreground). Teste de digitação fica manual.";
            return result;
        }

        const string Unit = "city.Log(\"Ação, coração, pão, você, à, útil, ç, õ, é!\"); ";
        string paced = Repeat(Unit, 400);
        string burst = Repeat(Unit, 400);

        int typed = TypeGuarded(editor, paced, delayMs: 100);       // ~120 palavras/min
        if (typed == paced.Length)
        {
            typed += TypeGuarded(editor, burst, delayMs: 2);        // rajada
        }

        Thread.Sleep(1000);
        JsonElement value = await RequestAsync("getValue", "value", TimeSpan.FromSeconds(10));
        string expected = (paced + burst)[..typed];
        string actual = value.GetString() ?? string.Empty;

        result["charsSent"] = typed;
        result["abortedBecauseFocusLost"] = typed < paced.Length + burst.Length;
        result["identical"] = actual == expected;
        result["receivedLength"] = actual.Length;
        result["firstMismatchIndex"] = FirstMismatch(expected, actual);
        return result;
    }

    // ------------------------------------------------------------------------------------------
    // Editor embutido: janela-filha (WS_CHILD) do jogo, ocupando o painel lateral direito.
    // ------------------------------------------------------------------------------------------
    private const int HudHeight = 40;          // barra superior do jogo; o painel começa abaixo dela
    private const int CollapsedWidth = 44;     // faixa do painel minimizado
    private const double NormalFraction = 0.34;
    private const double ExpandedFraction = 0.70;

    private void EmbedAsChild(IntPtr editor, IntPtr game)
    {
        _window.Invoke(() =>
        {
            Native.SetWindowLongPtrW(editor, Native.GwlpHwndParent, IntPtr.Zero); // desfaz um dono anterior
            long style = Native.GetWindowLongPtrW(editor, Native.GwlStyle).ToInt64();
            style &= ~(Native.WsPopup | Native.WsCaption | Native.WsThickFrame | Native.WsSysMenu | Native.WsMinimizeBox | Native.WsMaximizeBox);
            style |= Native.WsChild | Native.WsClipSiblings;
            Native.SetWindowLongPtrW(editor, Native.GwlStyle, new IntPtr(style));
            Native.SetParent(editor, game);
        });
        _embedded = true;
        PlaceInRightPanel(editor, game);
    }

    /// <summary>Retângulo do painel (coordenadas do cliente do jogo) para o estado atual.</summary>
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

    private async Task<JsonObject> EmbeddedTestAsync()
    {
        IntPtr editor = _window.WindowHandle;
        IntPtr game = _game!.Handle;
        var result = new JsonObject();

        // O jogo recorta os filhos ao desenhar (WS_CLIPCHILDREN), para não pintar por cima do painel.
        long gameStyle = Native.GetWindowLongPtrW(game, Native.GwlStyle).ToInt64() | Native.WsClipChildren;
        Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(gameStyle));

        long start = Stopwatch.GetTimestamp();
        EmbedAsChild(editor, game);
        result["embedMs"] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        Thread.Sleep(500);
        result["isChildOfGame"] = Native.IsChild(game, editor);
        result["visible"] = Native.IsWindowVisible(editor);
        result["panelMatchesExpected"] = PanelMatches(editor, game);
        result["pingGameMs"] = Native.Ping(game);
        result["pingEditorMs"] = Native.Ping(editor);
        result["typingWindowed"] = await TypeInEmbeddedAsync(editor, game, chars: 300);

        // Redimensionar o jogo: o filho não se ajusta sozinho; o host reposiciona (no produto, a Unity informa o retângulo do painel).
        Native.GetWindowRect(game, out Native.Rect g);
        Native.SetWindowPos(game, IntPtr.Zero, g.Left, g.Top, g.Right - g.Left + 200, g.Bottom - g.Top + 100, Native.SwpNoZOrder | Native.SwpNoActivate);
        Thread.Sleep(300);
        bool matchedBefore = PanelMatches(editor, game);
        PlaceInRightPanel(editor, game);
        Thread.Sleep(200);
        result["resize"] = new JsonObject { ["panelMatchedBeforeReposition"] = matchedBefore, ["panelMatchesAfterReposition"] = PanelMatches(editor, game) };

        // Tela cheia sem bordas (equivalente ao FullScreenMode.FullScreenWindow da Unity).
        Native.GetWindowRect(game, out Native.Rect windowed);
        long borderless = gameStyle & ~(Native.WsCaption | Native.WsThickFrame | Native.WsSysMenu | Native.WsMinimizeBox | Native.WsMaximizeBox);
        Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(borderless));
        int screenWidth = Native.GetSystemMetrics(Native.SmCxScreen);
        int screenHeight = Native.GetSystemMetrics(Native.SmCyScreen);
        Native.SetWindowPos(game, IntPtr.Zero, 0, 0, screenWidth, screenHeight, Native.SwpNoZOrder | Native.SwpFrameChanged | Native.SwpShowWindow);
        Thread.Sleep(600);
        PlaceInRightPanel(editor, game);
        Thread.Sleep(300);
        result["fullscreenBorderless"] = new JsonObject
        {
            ["screen"] = $"{screenWidth}x{screenHeight}",
            ["isChildOfGame"] = Native.IsChild(game, editor),
            ["visible"] = Native.IsWindowVisible(editor),
            ["panelMatchesExpected"] = PanelMatches(editor, game),
            ["typing"] = await TypeInEmbeddedAsync(editor, game, chars: 200),
        };

        // De volta para janela.
        Native.SetWindowLongPtrW(game, Native.GwlStyle, new IntPtr(gameStyle));
        Native.SetWindowPos(game, IntPtr.Zero, windowed.Left, windowed.Top, windowed.Right - windowed.Left, windowed.Bottom - windowed.Top, Native.SwpNoZOrder | Native.SwpFrameChanged);
        Thread.Sleep(500);
        PlaceInRightPanel(editor, game);
        Thread.Sleep(200);
        result["backToWindowed"] = new JsonObject { ["visible"] = Native.IsWindowVisible(editor), ["panelMatchesExpected"] = PanelMatches(editor, game) };
        result["pingGameAfterMs"] = Native.Ping(game);
        result["pingEditorAfterMs"] = Native.Ping(editor);
        return result;
    }

    /// <summary>
    /// Embutido como filho, o foco do teclado não entra sozinho no WebView2. Testa três formas de entregá-lo,
    /// cada uma com uma sonda de 30 caracteres, e faz o teste completo com a primeira que funcionar.
    /// </summary>
    private async Task<JsonObject> TypeInEmbeddedAsync(IntPtr editor, IntPtr game, int chars)
    {
        var result = new JsonObject { ["foregroundGranted"] = Native.SetForegroundWindow(game) };
        Thread.Sleep(300);
        if (!Native.IsForeground(game))
        {
            result["skipped"] = true;
            result["reason"] = "o jogo falso não ficou em primeiro plano";
            return result;
        }

        string? working = null;
        var probes = new JsonObject();
        foreach (string strategy in new[] { "setFocusPanel", "sendActivate", "setFocusChromium", "guardedClick" })
        {
            bool applied = FocusEmbedded(strategy, editor, game);
            await RequestAsync("prepareTyping", "typingReady", TimeSpan.FromSeconds(10));
            Thread.Sleep(300);
            string focusedClass = string.Empty;
            _window.Invoke(() => focusedClass = Native.ClassName(Native.GetFocus()));
            string probe = Repeat("ação ", 30);
            int sent = applied ? TypeGuarded(game, probe, delayMs: 20) : 0;
            Thread.Sleep(500);
            string got = (await RequestAsync("getValue", "value", TimeSpan.FromSeconds(10))).GetString() ?? string.Empty;
            probes[strategy] = new JsonObject
            {
                ["applied"] = applied,
                ["focusedWindowClass"] = focusedClass,
                ["hitTest"] = _lastHitTest,
                ["sent"] = sent,
                ["received"] = got.Length,
            };
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
            return result;
        }

        FocusEmbedded(working, editor, game);
        await RequestAsync("prepareTyping", "typingReady", TimeSpan.FromSeconds(10));
        Thread.Sleep(300);

        string text = Repeat("city.Log(\"Ação, você, pão, ç!\"); ", chars);
        int typed = TypeGuarded(game, text, delayMs: 20);
        Thread.Sleep(800);
        string actual = (await RequestAsync("getValue", "value", TimeSpan.FromSeconds(10))).GetString() ?? string.Empty;
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
                // Janela-filha nunca recebe WM_ACTIVATE; o PhotinoX pode depender dele para mover o foco ao WebView2.
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
                // Clica no centro do painel só se o ponto estiver sobre o nosso editor (nunca em outro app).
                Native.GetWindowRect(editor, out Native.Rect r);
                var point = new Native.Point { X = (r.Left + r.Right) / 2, Y = (r.Top + r.Bottom) / 2 };
                IntPtr hit = Native.WindowFromPoint(point);
                IntPtr hitRoot = Native.GetAncestor(hit, Native.GaRoot);
                _lastHitTest = $"hit={Native.ClassName(hit)} root={(hitRoot == game ? "jogo" : Native.ClassName(hitRoot))} childOfEditor={Native.IsChild(editor, hit)}";
                if (hitRoot != game || (hit != editor && !Native.IsChild(editor, hit)))
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
        Native.MapWindowPoints(IntPtr.Zero, game, ref e, 2); // tela → cliente do jogo
        return Math.Abs(e.Left - p.Left) <= 1 && Math.Abs(e.Top - p.Top) <= 1
            && Math.Abs(e.Right - p.Right) <= 1 && Math.Abs(e.Bottom - p.Bottom) <= 1;
    }

    // ------------------------------------------------------------------------------------------
    // Capturas de tela dos três estados do painel (normal, expandido, minimizado).
    // ------------------------------------------------------------------------------------------
    private async Task ScreenshotsAsync()
    {
        IntPtr editor = _window.WindowHandle;
        IntPtr game = _game!.Handle;
        var shots = new JsonArray();
        try
        {
            Directory.CreateDirectory(_screenshotDir!);
            EmbedAsChild(editor, game);
            await RequestAsync("demo", "demoReady", TimeSpan.FromSeconds(10));
            Thread.Sleep(1800); // diagnósticos (debounce) + console da demonstração

            foreach (string state in new[] { "normal", "expanded", "minimized" })
            {
                await RequestAsync("setState", state, "stateApplied", TimeSpan.FromSeconds(10));
                _panelState = state;
                PlaceInRightPanel(editor, game);
                Native.SetForegroundWindow(game);
                Thread.Sleep(900);
                string path = Path.Combine(_screenshotDir!, "panel-" + state + ".png");
                if (OperatingSystem.IsWindows())
                {
                    Capture(game, path);
                    shots.Add(path);
                }
            }
        }
        catch (Exception ex)
        {
            _results["fatal"] = ex.ToString();
        }

        _results["screenshots"] = shots;
        _results["pageErrors"] = new JsonArray(_pageErrors.Select(e => (JsonNode)e).ToArray());
        Finish();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void Capture(IntPtr window, string path)
    {
        Native.GetWindowRect(window, out Native.Rect r);
        using var bitmap = new Bitmap(r.Right - r.Left, r.Bottom - r.Top);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(r.Left, r.Top, 0, 0, bitmap.Size);
        }

        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }

    /// <summary>Injeta caracteres Unicode só enquanto <paramref name="topLevel"/> tem o foco (nunca digita em outro app).</summary>
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
            ["totalWorkingSetMB"] = MB(processes.Sum(p => p.WorkingSetBytes)),
            ["totalPrivateMB"] = MB(processes.Sum(p => p.PrivateBytes)),
            ["hostWorkingSetMB"] = MB(processes.Where(p => p.Pid == Environment.ProcessId).Sum(p => p.WorkingSetBytes)),
            ["hostPrivateMB"] = MB(processes.Where(p => p.Pid == Environment.ProcessId).Sum(p => p.PrivateBytes)),
            ["webView2PrivateMB"] = MB(processes.Where(p => p.Pid != Environment.ProcessId).Sum(p => p.PrivateBytes)),
            ["webView2Version"] = ProcessTree.WebView2Version(processes),
            ["processes"] = new JsonArray(processes
                .Select(p => (JsonNode)new JsonObject { ["name"] = p.Name, ["workingSetMB"] = MB(p.WorkingSetBytes), ["privateMB"] = MB(p.PrivateBytes) })
                .ToArray()),
        };
    }

    private JsonObject OwnerCloseTest()
    {
        IntPtr editor = _window.WindowHandle;
        _game!.Close();
        Thread.Sleep(1500);
        return new JsonObject { ["editorWindowStillExists"] = Native.IsWindow(editor) };
    }

    private void WriteResults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_outputPath)!);
        File.WriteAllText(_outputPath, _results.ToJsonString(Json));
    }

    private void Finish()
    {
        WriteResults();
        Console.WriteLine(_results.ToJsonString(Json));
        try
        {
            _window.Invoke(() => _window.Close());
        }
        catch (Exception)
        {
            // a janela já pode ter sido destruída junto com o dono
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(3000);
            Environment.Exit(0);
        });
    }

    private static string Repeat(string unit, int length)
    {
        var builder = new System.Text.StringBuilder(length + unit.Length);
        while (builder.Length < length)
        {
            builder.Append(unit);
        }

        return builder.ToString(0, length);
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
