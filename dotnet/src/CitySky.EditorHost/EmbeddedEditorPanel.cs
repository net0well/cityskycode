using System;
using System.Runtime.Versioning;
using System.Threading;
using Photino.NET;

namespace CitySky.EditorHost;

/// <summary>
/// Embute a janela do editor (PhotinoX/WebView2) como janela-filha (WS_CHILD) da janela do jogo,
/// ocupando o painel lateral direito — a técnica validada no spike S2a (o editor compõe por cima do
/// swapchain da Unity). Mantém o retângulo do painel sincronizado quando o jogo é redimensionado e
/// aplica os estados do painel (normal/expandido/minimizado) mudando só a largura. Se a janela do
/// jogo desaparecer, aciona o fechamento do editor.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class EmbeddedEditorPanel : IDisposable
{
    private const int CollapsedWidth = 44;
    private const double NormalFraction = 0.34;
    private const double ExpandedFraction = 0.70;
    private const int PollIntervalMs = 150;

    private readonly PhotinoWindow _window;
    private readonly UnityGameWindow _game;
    private readonly IntPtr _editorHandle;
    private readonly IntPtr _gameHandle;
    private readonly Action _onGameClosed;

    private Timer? _sync;
    private string _state = "normal";
    private Native.Rect _lastClient;
    private int _closed;

    public EmbeddedEditorPanel(PhotinoWindow window, UnityGameWindow game, Action onGameClosed)
    {
        _window = window;
        _game = game;
        _gameHandle = game.Handle;
        _editorHandle = window.WindowHandle;
        _onGameClosed = onGameClosed;
    }

    /// <summary>Reparenta o editor como filho do jogo, posiciona no painel direito e começa a sincronizar.</summary>
    public void Attach()
    {
        // O jogo precisa recortar os filhos ao desenhar, senão ele pinta por cima do editor.
        long gameStyle = Native.GetWindowLongPtrW(_gameHandle, Native.GwlStyle).ToInt64() | Native.WsClipChildren;
        Native.SetWindowLongPtrW(_gameHandle, Native.GwlStyle, new IntPtr(gameStyle));

        _window.Invoke(() =>
        {
            Native.SetWindowLongPtrW(_editorHandle, Native.GwlpHwndParent, IntPtr.Zero);
            long style = Native.GetWindowLongPtrW(_editorHandle, Native.GwlStyle).ToInt64();
            style &= ~(Native.WsPopup | Native.WsCaption | Native.WsThickFrame | Native.WsSysMenu | Native.WsMinimizeBox | Native.WsMaximizeBox);
            style |= Native.WsChild | Native.WsClipSiblings;
            Native.SetWindowLongPtrW(_editorHandle, Native.GwlStyle, new IntPtr(style));
            Native.SetParent(_editorHandle, _gameHandle);
        });

        Reposition(force: true);
        _sync = new Timer(_ => Sync(), null, PollIntervalMs, PollIntervalMs);
    }

    /// <summary>Aplica o estado do painel vindo do editor-web (normal/expanded/minimized).</summary>
    public void SetState(string state)
    {
        _state = state is "minimized" or "expanded" ? state : "normal";
        Reposition(force: true);
    }

    private void Sync()
    {
        if (!_game.IsAlive || !Native.IsWindow(_gameHandle))
        {
            if (Interlocked.Exchange(ref _closed, 1) == 0)
            {
                _onGameClosed();
            }

            return;
        }

        Reposition(force: false);
    }

    private void Reposition(bool force)
    {
        if (!Native.GetClientRect(_gameHandle, out Native.Rect client))
        {
            return;
        }

        // O retângulo do cliente nasce em (0,0); só a largura/altura mudam ao redimensionar o jogo.
        if (!force && client.Width == _lastClient.Width && client.Height == _lastClient.Height)
        {
            return;
        }

        _lastClient = client;
        Native.Rect p = PanelRect(client);
        _window.Invoke(() => Native.SetWindowPos(
            _editorHandle, IntPtr.Zero, p.Left, p.Top, p.Width, p.Height,
            Native.SwpNoZOrder | Native.SwpNoActivate | Native.SwpShowWindow));
    }

    private Native.Rect PanelRect(Native.Rect client)
    {
        int width = _state switch
        {
            "minimized" => CollapsedWidth,
            "expanded" => (int)(client.Width * ExpandedFraction),
            _ => (int)(client.Width * NormalFraction),
        };
        return new Native.Rect { Left = client.Width - width, Top = 0, Right = client.Width, Bottom = client.Height };
    }

    public void Dispose()
    {
        _sync?.Dispose();
        _game.Dispose();
    }
}
