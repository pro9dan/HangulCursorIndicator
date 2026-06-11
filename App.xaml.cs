using System.Windows;
using System.Windows.Threading;
using HangulCursorIndicator.Services;
using HangulCursorIndicator.Windows;
using MessageBox = System.Windows.MessageBox;

namespace HangulCursorIndicator;

public class App : System.Windows.Application
{
    private static Mutex? _singleInstanceMutex;
    private readonly ImeStatusService _imeStatusService = new();
    private readonly ToastWindowManager _toastWindowManager = new();
    private BadgeWindow? _badgeWindow;
    private TrayIconService? _trayIconService;
    private GlobalMessageHotkeyService? _messageHotkeyService;
    private LanMessageService? _lanMessageService;
    private SingleInstanceNotificationService? _singleInstanceNotificationService;
    private MessageInputWindow? _messageInputWindow;
    private DispatcherTimer? _timer;
    private bool _badgeEnabled = true;
    private bool _isOpeningMessageInput;

    [STAThread]
    public static void Main()
    {
        AppLogger.Initialize();
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                AppLogger.Error(exception, "Unhandled AppDomain exception");
            }
            else
            {
                AppLogger.Warn($"Unhandled AppDomain exception object: {args.ExceptionObject}");
            }
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppLogger.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };

        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: AppSettings.SingleInstanceMutexName,
            createdNew: out var createdNew);

        if (!createdNew)
        {
            AppLogger.Warn("Second instance launch blocked");
            if (SingleInstanceNotificationService.NotifyExistingInstance())
            {
                AppLogger.Info("Existing instance notified");
            }
            else
            {
                AppLogger.Warn("Existing instance notification was not ready");
            }

            _singleInstanceMutex.Dispose();
            return;
        }

        NativeMethods.EnablePerMonitorDpiAwareness();

        var app = new App();
        app.DispatcherUnhandledException += (_, args) =>
        {
            AppLogger.Error(args.Exception, "Unhandled dispatcher exception");
            args.Handled = true;
        };
        app.Run();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        _singleInstanceMutex = null;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppLogger.Info("Application startup");

        _badgeWindow = new BadgeWindow();
        _badgeWindow.Show();

        _trayIconService = new TrayIconService(
            isBadgeEnabled: () => _badgeEnabled,
            setBadgeEnabled: SetBadgeEnabled,
            exit: Shutdown);

        _singleInstanceNotificationService = new SingleInstanceNotificationService(() =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                AppLogger.Info("Showing already-running notification");
                MessageBox.Show(
                    "\uD504\uB85C\uADF8\uB7A8\uC774 \uC774\uBBF8 \uC2E4\uD589 \uC911\uC785\uB2C8\uB2E4.",
                    "Hangul Cursor Indicator",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        });

        _lanMessageService = new LanMessageService();
        _lanMessageService.MessageReceived += (_, message) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                AppLogger.Info($"Showing received toast from {message.Sender}, length={message.Text.Length}");
                _toastWindowManager.Show(message.Text, message.Sender);
            });
        };
        _lanMessageService.Start();

        _messageHotkeyService = new GlobalMessageHotkeyService(ShowMessageInput);
        _messageHotkeyService.Start();

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = AppSettings.RefreshInterval
        };
        _timer.Tick += (_, _) => RefreshBadge();
        _timer.Start();

        RefreshBadge();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppLogger.Info("Application exit");
        _timer?.Stop();
        _messageHotkeyService?.Dispose();
        _lanMessageService?.Dispose();
        _singleInstanceNotificationService?.Dispose();
        _trayIconService?.Dispose();
        base.OnExit(e);
    }

    private void SetBadgeEnabled(bool enabled)
    {
        _badgeEnabled = enabled;
        RefreshBadge();
    }

    private void RefreshBadge()
    {
        if (_badgeWindow is null)
        {
            return;
        }

        if (!_badgeEnabled)
        {
            _badgeWindow.HideBadge();
            return;
        }

        var status = _imeStatusService.GetCurrentStatus();
        var text = status == ImeInputStatus.Hangul ? AppSettings.HangulText : AppSettings.EnglishText;

        _badgeWindow.SetBadgeText(text);
        _badgeWindow.MoveNearCursor();
        _badgeWindow.ShowBadge();
    }

    private void ShowMessageInput()
    {
        AppLogger.Info("ShowMessageInput requested");
        if (_isOpeningMessageInput)
        {
            AppLogger.Info("ShowMessageInput skipped while opening");
            return;
        }

        if (_messageInputWindow is { IsVisible: true })
        {
            _messageInputWindow.Activate();
            _messageInputWindow.FocusTextBox();
            return;
        }

        _isOpeningMessageInput = true;
        _messageInputWindow = new MessageInputWindow(text =>
        {
            if (_lanMessageService is null)
            {
                AppLogger.Warn("LanMessageService is null while sending message");
                return;
            }

            AppLogger.Info($"Message submitted, length={text.Length}");
            _ = _lanMessageService.SendAsync(text);
        });
        _messageInputWindow.Closed += (_, _) =>
        {
            AppLogger.Info("Message input closed");
            _messageInputWindow = null;
            _isOpeningMessageInput = false;
        };
        _messageInputWindow.ShowNearCursor();
    }
}
