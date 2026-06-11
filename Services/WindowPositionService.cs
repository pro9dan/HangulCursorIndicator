using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace HangulCursorIndicator.Services;

public static class WindowPositionService
{
    public static void CenterOnCursorMonitor(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var cursorPoint = System.Windows.Forms.Cursor.Position;
        var screen = System.Windows.Forms.Screen.FromPoint(cursorPoint);
        var width = ToDevicePixels(window.ActualWidth > 0 ? window.ActualWidth : window.Width, window, isWidth: true);
        var height = ToDevicePixels(window.ActualHeight > 0 ? window.ActualHeight : window.Height, window, isWidth: false);

        var x = screen.WorkingArea.Left + (screen.WorkingArea.Width - width) / 2;
        var y = screen.WorkingArea.Top + (screen.WorkingArea.Height - height) / 2;

        NativeMethods.SetWindowPos(
            handle,
            NativeMethods.HwndTopMost,
            x,
            y,
            0,
            0,
            NativeMethods.SetWindowPosFlags.NoActivate |
            NativeMethods.SetWindowPosFlags.NoSize |
            NativeMethods.SetWindowPosFlags.ShowWindow);
    }

    private static int ToDevicePixels(double value, Visual visual, bool isWidth)
    {
        var source = PresentationSource.FromVisual(visual);
        var transform = source?.CompositionTarget?.TransformToDevice ?? System.Windows.Media.Matrix.Identity;
        var scale = isWidth ? transform.M11 : transform.M22;
        return Math.Max(1, (int)Math.Round(value * scale));
    }
}
