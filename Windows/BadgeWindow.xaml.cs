using System.Windows;
using System.Windows.Interop;
using HangulCursorIndicator.Services;

namespace HangulCursorIndicator.Windows;

public partial class BadgeWindow : Window
{
    public BadgeWindow()
    {
        InitializeComponent();
        Width = AppSettings.BadgeWidth;
        Height = AppSettings.BadgeHeight;
        Left = -10000;
        Top = -10000;
        SourceInitialized += (_, _) => ApplyWindowStyles();
    }

    public void SetBadgeText(string text)
    {
        if (BadgeText.Text != text)
        {
            BadgeText.Text = text;
        }
    }

    public void ShowBadge()
    {
        if (!IsVisible)
        {
            Show();
        }
    }

    public void HideBadge()
    {
        if (IsVisible)
        {
            Hide();
        }
    }

    public void MoveNearCursor()
    {
        if (!NativeMethods.GetCursorPos(out var cursor))
        {
            return;
        }

        var x = cursor.X + AppSettings.CursorOffsetX;
        var y = cursor.Y + AppSettings.CursorOffsetY;
        var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(cursor.X, cursor.Y));

        if (x + AppSettings.BadgeWidth > screen.WorkingArea.Right)
        {
            x = cursor.X - AppSettings.CursorOffsetX - AppSettings.BadgeWidth;
        }

        if (y + AppSettings.BadgeHeight > screen.WorkingArea.Bottom)
        {
            y = cursor.Y - AppSettings.CursorOffsetY - AppSettings.BadgeHeight;
        }

        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

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

    private void ApplyWindowStyles()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        // TOOLWINDOW keeps the badge out of Alt+Tab. TRANSPARENT and LAYERED make it click-through.
        var style = NativeMethods.GetWindowLongPtr(handle, NativeMethods.GwlExStyle).ToInt64();
        style |= NativeMethods.WsExToolWindow;
        style |= NativeMethods.WsExTransparent;
        style |= NativeMethods.WsExLayered;
        style |= NativeMethods.WsExNoActivate;
        NativeMethods.SetWindowLongPtr(handle, NativeMethods.GwlExStyle, new IntPtr(style));
    }
}
