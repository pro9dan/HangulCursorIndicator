namespace HangulCursorIndicator.Services;

public sealed class SingleInstanceNotificationService : IDisposable
{
    private readonly Action _notifyAlreadyRunning;
    private readonly EventWaitHandle _eventHandle;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _waitTask;

    public SingleInstanceNotificationService(Action notifyAlreadyRunning)
    {
        _notifyAlreadyRunning = notifyAlreadyRunning;
        _eventHandle = new EventWaitHandle(
            initialState: false,
            mode: EventResetMode.AutoReset,
            name: AppSettings.SingleInstanceNotifyEventName);
        _waitTask = Task.Run(WaitLoop);
    }

    public static bool NotifyExistingInstance()
    {
        try
        {
            using var eventHandle = EventWaitHandle.OpenExisting(AppSettings.SingleInstanceNotifyEventName);
            eventHandle.Set();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _eventHandle.Set();
        _eventHandle.Dispose();
        _cts.Dispose();
    }

    private void WaitLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                _eventHandle.WaitOne();
                if (!_cts.IsCancellationRequested)
                {
                    _notifyAlreadyRunning();
                }
            }
            catch
            {
                return;
            }
        }
    }
}
