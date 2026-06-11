namespace HangulCursorIndicator.Services;

public sealed record LanMessagePacket(
    string App,
    string InstanceId,
    string Sender,
    string Text,
    DateTimeOffset SentAt);
