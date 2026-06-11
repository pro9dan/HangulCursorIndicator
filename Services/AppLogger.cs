using System.IO;

namespace HangulCursorIndicator.Services;

public static class AppLogger
{
    private static readonly object Gate = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HangulCursorIndicator",
        "logs");

    public static string LogFilePath { get; private set; } = string.Empty;

    public static void Initialize()
    {
        Directory.CreateDirectory(LogDirectory);
        LogFilePath = Path.Combine(LogDirectory, $"{DateTime.Now:yyyyMMdd}.log");
        Info("Logger initialized");
    }

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Warn(string message)
    {
        Write("WARN", message);
    }

    public static void Error(Exception exception, string message)
    {
        Write("ERROR", $"{message}{Environment.NewLine}{exception}");
    }

    private static void Write(string level, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(LogFilePath))
            {
                Initialize();
            }

            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
            lock (Gate)
            {
                File.AppendAllText(LogFilePath, line);
            }
        }
        catch
        {
            // Logging must never crash the application.
        }
    }
}
