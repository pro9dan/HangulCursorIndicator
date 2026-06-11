using System.Windows.Interop;

namespace HangulCursorIndicator.Services;

public sealed class GlobalMessageHotkeyService : IDisposable
{
    private const int HotkeyId = 0x4843;
    private readonly Action _showInput;
    private HwndSource? _source;
    private bool _registered;

    public GlobalMessageHotkeyService(Action showInput)
    {
        _showInput = showInput;
    }

    public void Start()
    {
        if (_source is not null)
        {
            return;
        }

        var parameters = new HwndSourceParameters("HangulCursorIndicatorHotkey")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        _registered = NativeMethods.RegisterHotKey(
            _source.Handle,
            HotkeyId,
            NativeMethods.ModControl | NativeMethods.ModShift | NativeMethods.ModNoRepeat,
            NativeMethods.VkP);

        if (_registered)
        {
            AppLogger.Info("Ctrl+Shift+P hotkey registered");
        }
        else
        {
            AppLogger.Warn("Failed to register Ctrl+Shift+P hotkey");
        }
    }

    public void Dispose()
    {
        if (_source is not null)
        {
            if (_registered)
            {
                NativeMethods.UnregisterHotKey(_source.Handle, HotkeyId);
                _registered = false;
            }

            _source.RemoveHook(WndProc);
            _source.Dispose();
            _source = null;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            AppLogger.Info("Ctrl+Shift+P detected");
            _showInput();
        }

        return IntPtr.Zero;
    }
}
