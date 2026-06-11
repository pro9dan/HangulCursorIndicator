using System.Windows;
using System.Windows.Threading;
using HangulCursorIndicator.Services;
using HangulCursorIndicator.Windows;

namespace HangulCursorIndicator;

public class App : System.Windows.Application
{
    private readonly ImeStatusService _imeStatusService = new();
    private BadgeWindow? _badgeWindow;
    private TrayIconService? _trayIconService;
    private DispatcherTimer? _timer;
    private bool _badgeEnabled = true;

    [STAThread]
    public static void Main()
    {
        NativeMethods.EnablePerMonitorDpiAwareness();

        var app = new App();
        app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _badgeWindow = new BadgeWindow();
        _badgeWindow.Show();

        _trayIconService = new TrayIconService(
            isBadgeEnabled: () => _badgeEnabled,
            setBadgeEnabled: SetBadgeEnabled,
            exit: Shutdown);

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
        _timer?.Stop();
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
}
