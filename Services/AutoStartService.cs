using Microsoft.Win32;

namespace HangulCursorIndicator.Services;

public static class AutoStartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(AppSettings.AutoStartRunName) is string value &&
               string.Equals(value, GetExecutableCommand(), StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                        ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
                        ?? throw new InvalidOperationException("Unable to open the current user's Run registry key.");

        if (enabled)
        {
            key.SetValue(AppSettings.AutoStartRunName, GetExecutableCommand(), RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(AppSettings.AutoStartRunName, throwOnMissingValue: false);
        }
    }

    private static string GetExecutableCommand()
    {
        var path = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return $"\"{path}\"";
    }
}
