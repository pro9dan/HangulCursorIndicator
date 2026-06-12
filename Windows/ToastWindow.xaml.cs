using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using HangulCursorIndicator.Services;

namespace HangulCursorIndicator.Windows;

public partial class ToastWindow : Window
{
    public ToastWindow(string text)
    {
        InitializeComponent();
        Width = AppSettings.ToastWidth;
        MaxHeight = AppSettings.ToastMaxHeight;
        MessageText.Text = text;
        SourceInitialized += (_, _) => ApplyWindowStyles();
        Loaded += (_, _) => BeginToastLifecycle();
    }

    private void ApplyWindowStyles()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = NativeMethods.GetWindowLongPtr(handle, NativeMethods.GwlExStyle).ToInt64();
        style |= NativeMethods.WsExToolWindow;
        style |= NativeMethods.WsExTransparent;
        style |= NativeMethods.WsExLayered;
        style |= NativeMethods.WsExNoActivate;
        NativeMethods.SetWindowLongPtr(handle, NativeMethods.GwlExStyle, new IntPtr(style));
    }

    private async void BeginToastLifecycle()
    {
        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(140)));
        await Task.Delay(AppSettings.ToastDuration);

        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(450));
        fadeOut.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, fadeOut);
    }
}
