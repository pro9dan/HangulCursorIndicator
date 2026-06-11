using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using HangulCursorIndicator.Services;

namespace HangulCursorIndicator.Windows;

public partial class MessageInputWindow : Window
{
    private readonly Action<string> _sendMessage;
    private bool _isClosing;

    public MessageInputWindow(Action<string> sendMessage)
    {
        _sendMessage = sendMessage;
        InitializeComponent();
        Width = AppSettings.MessageInputWidth;
        Height = AppSettings.MessageInputHeight;
        SourceInitialized += (_, _) => ApplyWindowStyles();
        Deactivated += (_, _) => CloseOnce("deactivated");
    }

    public void ShowNearCursor()
    {
        AppLogger.Info("Message input show");
        Show();
        WindowPositionService.CenterOnCursorMonitor(this);
        BringInputToForeground();
        FocusTextBox();
        Dispatcher.BeginInvoke(BringInputToForeground, DispatcherPriority.Input);
        Dispatcher.BeginInvoke(FocusTextBox, DispatcherPriority.ApplicationIdle);
    }

    public void FocusTextBox()
    {
        if (_isClosing)
        {
            return;
        }

        MessageBox.Focusable = true;
        MessageBox.Focus();
        Keyboard.Focus(MessageBox);
        FocusManager.SetFocusedElement(this, MessageBox);
        MessageBox.CaretIndex = MessageBox.Text.Length;
    }

    private void BringInputToForeground()
    {
        if (_isClosing)
        {
            return;
        }

        Activate();

        var windowHandle = new WindowInteropHelper(this).Handle;
        if (windowHandle != IntPtr.Zero)
        {
            NativeMethods.SetForegroundWindow(windowHandle);
        }

        var textBoxHandle = ((HwndSource)PresentationSource.FromVisual(MessageBox)!).Handle;
        if (textBoxHandle != IntPtr.Zero)
        {
            NativeMethods.SetFocus(textBoxHandle);
        }
    }

    private void MessageBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            var text = MessageBox.Text.Trim();
            AppLogger.Info($"Message input Enter pressed, length={text.Length}");
            CloseOnce("enter");

            if (!string.IsNullOrWhiteSpace(text))
            {
                _sendMessage(text);
            }
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            AppLogger.Info("Message input Escape pressed");
            CloseOnce("escape");
        }
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        AppLogger.Info("Message input Escape pressed");
        CloseOnce("escape");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        AppLogger.Info("Message input close button clicked");
        CloseOnce("close-button");
    }

    private void CloseOnce(string reason)
    {
        if (_isClosing)
        {
            AppLogger.Info($"Message input close skipped while already closing, reason={reason}");
            return;
        }

        _isClosing = true;
        AppLogger.Info($"Message input close requested, reason={reason}");
        Close();
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
        NativeMethods.SetWindowLongPtr(handle, NativeMethods.GwlExStyle, new IntPtr(style));
    }
}
