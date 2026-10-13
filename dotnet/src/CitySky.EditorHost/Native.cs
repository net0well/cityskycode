using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CitySky.EditorHost;

/// <summary>
/// Interop Win32 usado para embutir a janela do editor como filha da janela do jogo (painel lateral).
/// Específico do Windows (ADR-0018: Windows primeiro; o específico de SO fica atrás de uma fronteira —
/// aqui, do modo embutido do host). Nenhuma chamada daqui roda fora do Windows: o modo embutido só é
/// acionado sob <see cref="OperatingSystem.IsWindows"/>.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class Native
{
    public const int GwlpHwndParent = -8;
    public const int GwlStyle = -16;

    public const long WsChild = 0x40000000;
    public const long WsPopup = 0x80000000;
    public const long WsCaption = 0x00C00000;
    public const long WsThickFrame = 0x00040000;
    public const long WsSysMenu = 0x00080000;
    public const long WsMinimizeBox = 0x00020000;
    public const long WsMaximizeBox = 0x00010000;
    public const long WsClipChildren = 0x02000000;
    public const long WsClipSiblings = 0x04000000;

    public const uint SwpNoZOrder = 0x0004;
    public const uint SwpNoActivate = 0x0010;
    public const uint SwpFrameChanged = 0x0020;
    public const uint SwpShowWindow = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly int Width => Right - Left;

        public readonly int Height => Bottom - Top;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int index, IntPtr newLong);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int index);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsChild(IntPtr parent, IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassNameW(IntPtr hWnd, [Out] char[] className, int maxCount);

    public static string ClassName(IntPtr hWnd)
    {
        var buffer = new char[256];
        int length = GetClassNameW(hWnd, buffer, buffer.Length);
        return length > 0 ? new string(buffer, 0, length) : string.Empty;
    }
}
