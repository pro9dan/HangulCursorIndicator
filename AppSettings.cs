namespace HangulCursorIndicator;

public static class AppSettings
{
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMilliseconds(50);

    public const int BadgeWidth = 36;
    public const int BadgeHeight = 28;
    public const int CursorOffsetX = 20;
    public const int CursorOffsetY = 22;

    public const string HangulText = "한";
    public const string EnglishText = "A";
    public const string AutoStartRunName = "HangulCursorIndicator";
}
