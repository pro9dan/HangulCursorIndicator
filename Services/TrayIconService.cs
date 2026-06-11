using System.Drawing;
using System.Windows.Forms;

namespace HangulCursorIndicator.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleBadgeItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly Func<bool> _isBadgeEnabled;
    private readonly Action<bool> _setBadgeEnabled;
    private readonly Action _exit;
    private readonly Icon _icon;

    public TrayIconService(Func<bool> isBadgeEnabled, Action<bool> setBadgeEnabled, Action exit)
    {
        _isBadgeEnabled = isBadgeEnabled;
        _setBadgeEnabled = setBadgeEnabled;
        _exit = exit;

        _toggleBadgeItem = new ToolStripMenuItem();
        _toggleBadgeItem.Click += (_, _) =>
        {
            _setBadgeEnabled(!_isBadgeEnabled());
            UpdateMenuState();
        };

        _autoStartItem = new ToolStripMenuItem();
        _autoStartItem.Click += (_, _) =>
        {
            AutoStartService.SetEnabled(!AutoStartService.IsEnabled());
            UpdateMenuState();
        };

        var exitItem = new ToolStripMenuItem("\uC885\uB8CC");
        exitItem.Click += (_, _) => _exit();

        var menu = new ContextMenuStrip();
        menu.Opening += (_, _) => UpdateMenuState();
        menu.Items.Add(_toggleBadgeItem);
        menu.Items.Add(_autoStartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _icon = CreateTrayIcon();
        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "\uD55C/A \uC785\uB825 \uC0C1\uD0DC \uD45C\uC2DC",
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) =>
        {
            _setBadgeEnabled(!_isBadgeEnabled());
            UpdateMenuState();
        };

        UpdateMenuState();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
    }

    private void UpdateMenuState()
    {
        _toggleBadgeItem.Text = _isBadgeEnabled()
            ? "\uD45C\uC2DC \uB044\uAE30"
            : "\uD45C\uC2DC \uCF1C\uAE30";

        _autoStartItem.Text = AutoStartService.IsEnabled()
            ? "\uD504\uB85C\uADF8\uB7A8 \uC2DC\uC791 \uC2DC \uC790\uB3D9 \uC2E4\uD589 \uB044\uAE30"
            : "\uD504\uB85C\uADF8\uB7A8 \uC2DC\uC791 \uC2DC \uC790\uB3D9 \uC2E4\uD589 \uCF1C\uAE30";
    }

    private static Icon CreateTrayIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var backgroundBrush = new SolidBrush(Color.FromArgb(230, 32, 32, 32));
        graphics.FillEllipse(backgroundBrush, 2, 2, 28, 28);

        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
        var textSize = graphics.MeasureString("A", font);
        graphics.DrawString("A", font, textBrush, (32 - textSize.Width) / 2, (32 - textSize.Height) / 2 - 1);

        var handle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(handle);
        }
    }
}
