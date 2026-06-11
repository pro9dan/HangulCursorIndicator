using System.Runtime.InteropServices;

namespace HangulCursorIndicator.Services;

public static class NativeMethods
{
    public const int GwlExStyle = -20;
    public const long WsExTransparent = 0x00000020L;
    public const long WsExToolWindow = 0x00000080L;
    public const long WsExLayered = 0x00080000L;
    public const long WsExNoActivate = 0x08000000L;

    public const int WmImeControl = 0x0283;
    public const int WmHotkey = 0x0312;
    public const uint ModShift = 0x0004;
    public const uint ModControl = 0x0002;
    public const uint ModNoRepeat = 0x4000;
    public const uint VkControl = 0x11;
    public const uint VkShift = 0x10;
    public const uint VkP = 0x50;
    public static readonly IntPtr ImcGetConversionMode = new(0x0001);
    public const int ImeCmodeNative = 0x0001;

    public static readonly IntPtr HwndTopMost = new(-1);
    private static readonly IntPtr DpiAwarenessContextPerMonitorAwareV2 = new(-4);

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        NoSize = 0x0001,
        NoActivate = 0x0010,
        ShowWindow = 0x0040
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    [Flags]
    public enum SendMessageTimeoutFlags : uint
    {
        AbortIfHung = 0x0002
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GuiThreadInfo
    {
        public int CbSize;
        public uint Flags;
        public IntPtr HwndActive;
        public IntPtr HwndFocus;
        public IntPtr HwndCapture;
        public IntPtr HwndMenuOwner;
        public IntPtr HwndMoveSize;
        public IntPtr HwndCaret;
        public Rect RcCaret;

        public static GuiThreadInfo Create()
        {
            return new GuiThreadInfo
            {
                CbSize = Marshal.SizeOf<GuiThreadInfo>()
            };
        }
    }

    public static void EnablePerMonitorDpiAwareness()
    {
        if (!SetProcessDpiAwarenessContext(DpiAwarenessContextPerMonitorAwareV2))
        {
            SetProcessDPIAware();
        }
    }

    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(uint vKey);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetGUIThreadInfo(uint idThread, ref GuiThreadInfo guiThreadInfo);

    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("imm32.dll")]
    public static extern bool ImmGetOpenStatus(IntPtr hIMC);

    [DllImport("imm32.dll")]
    public static extern bool ImmGetConversionStatus(IntPtr hIMC, out int conversionMode, out int sentenceMode);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SendMessageTimeout(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        SendMessageTimeoutFlags flags,
        uint timeout,
        out IntPtr result);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        SetWindowPosFlags flags);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}
