namespace HangulCursorIndicator.Windows;

public sealed class ToastWindowManager
{
    public void Show(string text, string sender)
    {
        var toast = new ToastWindow(text);
        toast.Show();
    }
}
