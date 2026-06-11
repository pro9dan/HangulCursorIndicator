namespace HangulCursorIndicator;

public static class AppSettings
{
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(50);
    public static readonly TimeSpan ToastDuration = TimeSpan.FromSeconds(3);

    public const int BadgeWidth = 36;
    public const int BadgeHeight = 28;
    public const int CursorOffsetX = 20;
    public const int CursorOffsetY = 22;
    public const int MessageInputWidth = 320;
    public const int MessageInputHeight = 52;
    public const int MessageInputOffsetX = 20;
    public const int MessageInputOffsetY = 22;
    public const int ToastWidth = 360;
    public const int ToastMaxHeight = 160;
    public const int ToastBottomOffset = 72;
    public const int ToastRightOffset = 28;
    public const int MessagePort = 45455;
    public const int MaxMessageLength = 500;

    public const string HangulText = "\uD55C";
    public const string EnglishText = "A";
    public const string AutoStartRunName = "HangulCursorIndicator";
    public const string SingleInstanceMutexName = @"Local\HangulCursorIndicator.SingleInstance";
    public const string SingleInstanceNotifyEventName = @"Local\HangulCursorIndicator.ShowAlreadyRunning";
}
