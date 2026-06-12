using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HangulCursorIndicator.Services;

namespace HangulCursorIndicator.Windows;

public sealed class ToastWindowManager
{
    private const int ToastGap = 8;
    private readonly List<ToastWindow> _activeToasts = [];

    public void Show(string text, string sender)
    {
        var toast = new ToastWindow(text);
        _activeToasts.Add(toast);

        toast.ContentRendered += (_, _) => PositionToasts();
        toast.Closed += (_, _) =>
        {
            _activeToasts.Remove(toast);
            PositionToasts();
        };

        toast.Show();
    }

    private void PositionToasts()
    {
        var visibleToasts = _activeToasts
            .Where(toast => toast.IsVisible && new WindowInteropHelper(toast).Handle != IntPtr.Zero)
            .ToList();

        if (visibleToasts.Count == 0)
        {
            return;
        }

        var cursorPoint = System.Windows.Forms.Cursor.Position;
        var screen = System.Windows.Forms.Screen.FromPoint(cursorPoint);
        var heights = visibleToasts
            .Select(toast => ToDevicePixels(GetWindowHeight(toast), toast, isWidth: false))
            .ToList();

        var totalHeight = heights.Sum() + ToastGap * (visibleToasts.Count - 1);
        var firstHeight = heights[0];
        var top = screen.WorkingArea.Top + (screen.WorkingArea.Height - firstHeight) / 2;
        var overflow = top + totalHeight - screen.WorkingArea.Bottom;

        if (overflow > 0)
        {
            top -= overflow;
        }

        top = Math.Max(screen.WorkingArea.Top, top);

        for (var index = 0; index < visibleToasts.Count; index++)
        {
            var toast = visibleToasts[index];
            var width = ToDevicePixels(GetWindowWidth(toast), toast, isWidth: true);
            var left = screen.WorkingArea.Left + (screen.WorkingArea.Width - width) / 2;

            SetWindowPosition(toast, left, top);
            top += heights[index] + ToastGap;
        }
    }

    private static double GetWindowWidth(Window window)
    {
        return window.ActualWidth > 0 ? window.ActualWidth : window.Width;
    }

    private static double GetWindowHeight(Window window)
    {
        return window.ActualHeight > 0 ? window.ActualHeight : window.Height;
    }

    private static int ToDevicePixels(double value, Visual visual, bool isWidth)
    {
        var source = PresentationSource.FromVisual(visual);
        var transform = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        var scale = isWidth ? transform.M11 : transform.M22;
        return Math.Max(1, (int)Math.Round(value * scale));
    }

    private static void SetWindowPosition(Window window, int left, int top)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.SetWindowPos(
            handle,
            NativeMethods.HwndTopMost,
            left,
            top,
            0,
            0,
            NativeMethods.SetWindowPosFlags.NoActivate |
            NativeMethods.SetWindowPosFlags.NoSize |
            NativeMethods.SetWindowPosFlags.ShowWindow);
    }
}
