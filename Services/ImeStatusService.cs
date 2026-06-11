namespace HangulCursorIndicator.Services;

public sealed class ImeStatusService
{
    private const int KoreanLangId = 0x0412;
    private ImeInputStatus _lastKoreanImeStatus = ImeInputStatus.English;

    public ImeInputStatus GetCurrentStatus()
    {
        var foreground = NativeMethods.GetForegroundWindow();
        if (foreground == IntPtr.Zero)
        {
            return ImeInputStatus.English;
        }

        var threadId = NativeMethods.GetWindowThreadProcessId(foreground, out _);
        var inputWindow = GetFocusedInputWindow(threadId, foreground);
        var keyboardLayout = GetKeyboardLayoutForInputThread(threadId, inputWindow);
        if (!IsKoreanLayout(keyboardLayout))
        {
            _lastKoreanImeStatus = ImeInputStatus.English;
            return ImeInputStatus.English;
        }

        // The focused child HWND usually owns the real HIMC. This is more reliable than querying
        // only the foreground top-level window, especially after typing into Notepad/Edit controls.
        if (TryGetImmContextStatus(inputWindow, out var contextStatus))
        {
            _lastKoreanImeStatus = contextStatus;
            return contextStatus;
        }

        if (inputWindow != foreground && TryGetImmContextStatus(foreground, out contextStatus))
        {
            _lastKoreanImeStatus = contextStatus;
            return contextStatus;
        }

        // Some TSF-backed apps do not expose a usable HIMC. The default IME window is a fallback
        // path for WM_IME_CONTROL/IMC_GETCONVERSIONMODE.
        if (TryGetDefaultImeWindowStatus(inputWindow, out var imeWindowStatus) ||
            TryGetDefaultImeWindowStatus(foreground, out imeWindowStatus))
        {
            _lastKoreanImeStatus = imeWindowStatus;
            return imeWindowStatus;
        }

        return _lastKoreanImeStatus;
    }

    private static bool IsKoreanLayout(IntPtr keyboardLayout)
    {
        return (keyboardLayout.ToInt64() & 0xffff) == KoreanLangId;
    }

    private static IntPtr GetKeyboardLayoutForInputThread(uint threadId, IntPtr inputWindow)
    {
        var keyboardLayout = NativeMethods.GetKeyboardLayout(threadId);
        if (IsKoreanLayout(keyboardLayout))
        {
            return keyboardLayout;
        }

        // Some applications keep the real focused control on a different UI thread than the
        // foreground owner HWND. Query that focus HWND's thread as an additional fallback.
        if (inputWindow != IntPtr.Zero)
        {
            var inputThreadId = NativeMethods.GetWindowThreadProcessId(inputWindow, out _);
            if (inputThreadId != 0 && inputThreadId != threadId)
            {
                var inputKeyboardLayout = NativeMethods.GetKeyboardLayout(inputThreadId);
                if (IsKoreanLayout(inputKeyboardLayout))
                {
                    return inputKeyboardLayout;
                }
            }
        }

        return keyboardLayout;
    }

    private static IntPtr GetFocusedInputWindow(uint threadId, IntPtr fallback)
    {
        var info = NativeMethods.GuiThreadInfo.Create();
        if (NativeMethods.GetGUIThreadInfo(threadId, ref info) && info.HwndFocus != IntPtr.Zero)
        {
            return info.HwndFocus;
        }

        return fallback;
    }

    private static bool TryGetImmContextStatus(IntPtr hwnd, out ImeInputStatus status)
    {
        status = ImeInputStatus.English;

        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        var context = NativeMethods.ImmGetContext(hwnd);
        if (context == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            if (!NativeMethods.ImmGetOpenStatus(context))
            {
                status = ImeInputStatus.English;
                return true;
            }

            if (!NativeMethods.ImmGetConversionStatus(context, out var conversionMode, out _))
            {
                return false;
            }

            status = (conversionMode & NativeMethods.ImeCmodeNative) != 0
                ? ImeInputStatus.Hangul
                : ImeInputStatus.English;

            return true;
        }
        finally
        {
            NativeMethods.ImmReleaseContext(hwnd, context);
        }
    }

    private static bool TryGetDefaultImeWindowStatus(IntPtr hwnd, out ImeInputStatus status)
    {
        status = ImeInputStatus.English;

        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        var imeWindow = NativeMethods.ImmGetDefaultIMEWnd(hwnd);
        if (imeWindow == IntPtr.Zero)
        {
            return false;
        }

        if (!NativeMethods.SendMessageTimeout(
                imeWindow,
                NativeMethods.WmImeControl,
                NativeMethods.ImcGetConversionMode,
                IntPtr.Zero,
                NativeMethods.SendMessageTimeoutFlags.AbortIfHung,
                50,
                out var conversionMode))
        {
            return false;
        }

        return (conversionMode.ToInt64() & NativeMethods.ImeCmodeNative) != 0
            ? SetStatus(ImeInputStatus.Hangul, out status)
            : SetStatus(ImeInputStatus.English, out status);
    }

    private static bool SetStatus(ImeInputStatus value, out ImeInputStatus status)
    {
        status = value;
        return true;
    }
}
