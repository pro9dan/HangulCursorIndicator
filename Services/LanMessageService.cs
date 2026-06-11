using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace HangulCursorIndicator.Services;

public sealed class LanMessageService : IDisposable
{
    private const string AppName = "HangulCursorIndicator";
    private readonly string _instanceId = Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource _cts = new();
    private UdpClient? _receiver;
    private Task? _receiveTask;

    public event EventHandler<LanMessage>? MessageReceived;

    public static string LocalSenderName { get; } =
        $"{Environment.UserName}@{Environment.MachineName}";

    public void Start()
    {
        if (_receiver is not null)
        {
            return;
        }

        try
        {
            _receiver = new UdpClient(AddressFamily.InterNetwork);
            _receiver.EnableBroadcast = true;
            _receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _receiver.Client.Bind(new IPEndPoint(IPAddress.Any, AppSettings.MessagePort));
            _receiveTask = Task.Run(ReceiveLoopAsync);
            AppLogger.Info($"LAN message listener started on UDP {AppSettings.MessagePort}");
        }
        catch (Exception exception)
        {
            AppLogger.Error(exception, "Failed to start LAN message listener");
            _receiver?.Dispose();
            _receiver = null;
        }
    }

    public async Task SendAsync(string text)
    {
        var normalized = NormalizeMessage(text);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var packet = new LanMessagePacket(
            AppName,
            _instanceId,
            LocalSenderName,
            normalized,
            DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(packet);
        var bytes = Encoding.UTF8.GetBytes(json);

        try
        {
            using var sender = new UdpClient(AddressFamily.InterNetwork)
            {
                EnableBroadcast = true
            };
            await sender.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, AppSettings.MessagePort));
            AppLogger.Info($"UDP broadcast sent, bytes={bytes.Length}");
        }
        catch (Exception exception)
        {
            // Network broadcast can fail because of firewall, adapter, or policy settings.
            // The message is intentionally ephemeral, so keep the app alive and drop it.
            AppLogger.Error(exception, "UDP broadcast failed");
        }
    }

    public void Dispose()
    {
        AppLogger.Info("Disposing LAN message service");
        _cts.Cancel();
        _receiver?.Dispose();
        _cts.Dispose();
    }

    private async Task ReceiveLoopAsync()
    {
        if (_receiver is null)
        {
            return;
        }

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var result = await _receiver.ReceiveAsync(_cts.Token);
                var json = Encoding.UTF8.GetString(result.Buffer);
                var packet = JsonSerializer.Deserialize<LanMessagePacket>(json);

                if (packet is null ||
                    packet.App != AppName ||
                    packet.InstanceId == _instanceId ||
                    string.IsNullOrWhiteSpace(packet.Text))
                {
                    continue;
                }

                AppLogger.Info($"UDP message received from {packet.Sender}, length={packet.Text.Length}");
                MessageReceived?.Invoke(this, new LanMessage(NormalizeMessage(packet.Text), packet.Sender));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception exception)
            {
                // Ignore malformed UDP datagrams and keep listening.
                AppLogger.Error(exception, "Failed to process UDP datagram");
            }
        }
    }

    private static string NormalizeMessage(string text)
    {
        var normalized = text.Trim();
        return normalized.Length <= AppSettings.MaxMessageLength
            ? normalized
            : normalized[..AppSettings.MaxMessageLength];
    }
}
