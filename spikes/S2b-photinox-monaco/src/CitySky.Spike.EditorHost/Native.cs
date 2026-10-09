using System.Runtime.InteropServices;

namespace CitySky.Spike.EditorHost;

/// <summary>Win32 usado pelo spike (específico do Windows; no produto fica atrás de ICodeEditorHost).</summary>
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
    public const uint SwpFrameChanged = 0x0020;
    public const uint SwpShowWindow = 0x0040;
    public const int SmCxScreen = 0;
    public const int SmCyScreen = 1;
    public const int SwMinimize = 6;
    public const int SwRestore = 9;
    public const uint GwHwndNext = 2;
    public const uint GaRoot = 2;
    public const uint WmNull = 0x0000;
    public const uint WmActivate = 0x0006;
    public const int WaActive = 1;
    public const uint SmtoAbortIfHung = 0x0002;
    public const uint SwpNoSize = 0x0001;
    public const uint SwpNoZOrder = 0x0004;
    public const uint SwpNoActivate = 0x0010;
    public const uint InputKeyboard = 1;
    public const uint KeyeventfKeyUp = 0x0002;
    public const uint KeyeventfUnicode = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    /// <summary>INPUT do Win32: o union tem o tamanho de MOUSEINPUT, por isso Size = 40 no x64.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public struct Input
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(8)] public KeyboardInput Keyboard;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int index, IntPtr newLong);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int index);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint command);

    [DllImport("user32.dll")]
    public static extern IntPtr GetAncestor(IntPtr hWnd, uint flags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessageTimeoutW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeoutMs, out IntPtr result);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint count, Input[] inputs, int size);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsChild(IntPtr parent, IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetFocus();

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MapWindowPoints(IntPtr from, IntPtr to, ref Rect rect, int count);

    public const uint InputMouse = 0;
    public const uint MouseeventfLeftDown = 0x0002;
    public const uint MouseeventfLeftUp = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public struct MouseInputUnion
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(8)] public MouseInput Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumChildWindows(IntPtr parent, EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassNameW(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);

    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(Point point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out Point point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SendInput")]
    public static extern uint SendMouseInput(uint count, MouseInputUnion[] inputs, int size);

    public static string ClassName(IntPtr hWnd)
    {
        var builder = new System.Text.StringBuilder(256);
        GetClassNameW(hWnd, builder, builder.Capacity);
        return builder.ToString();
    }

    /// <summary>Janela do Chromium que recebe o teclado dentro do WebView2.</summary>
    public static IntPtr FindChromiumInputWindow(IntPtr root)
    {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(root, (h, _) =>
        {
            if (ClassName(h) == "Chrome_RenderWidgetHostHWND")
            {
                found = h;
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return found;
    }

    /// <summary>A janela de topo que tem o foco é <paramref name="hWnd"/>?</summary>
    public static bool IsForeground(IntPtr hWnd) => GetAncestor(GetForegroundWindow(), GaRoot) == hWnd;

    /// <summary><paramref name="upper"/> está acima de <paramref name="lower"/> na ordem Z?</summary>
    public static bool IsAbove(IntPtr upper, IntPtr lower)
    {
        for (IntPtr h = GetWindow(upper, GwHwndNext); h != IntPtr.Zero; h = GetWindow(h, GwHwndNext))
        {
            if (h == lower)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Tempo de resposta a WM_NULL (ms) ou -1 se não responder em 1 s.</summary>
    public static double Ping(IntPtr hWnd)
    {
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        IntPtr ok = SendMessageTimeoutW(hWnd, WmNull, IntPtr.Zero, IntPtr.Zero, SmtoAbortIfHung, 1000, out _);
        return ok == IntPtr.Zero ? -1 : System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    }
}
