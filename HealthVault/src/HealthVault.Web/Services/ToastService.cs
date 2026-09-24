namespace HealthVault.Web.Services;

public class ToastService
{
    public event Action<Toast>? OnShow;
    
    public void ShowSuccess(string message, string? title = null, int durationMs = 5000)
    {
        Show(ToastType.Success, message, title, durationMs);
    }
    
    public void ShowError(string message, string? title = null, int durationMs = 7000)
    {
        Show(ToastType.Error, message, title, durationMs);
    }
    
    public void ShowInfo(string message, string? title = null, int durationMs = 5000)
    {
        Show(ToastType.Info, message, title, durationMs);
    }
    
    public void ShowWarning(string message, string? title = null, int durationMs = 6000)
    {
        Show(ToastType.Warning, message, title, durationMs);
    }
    
    private void Show(ToastType type, string message, string? title, int durationMs)
    {
        var toast = new Toast
        {
            Id = Guid.NewGuid().ToString(),
            Type = type,
            Message = message,
            Title = title,
            DurationMs = durationMs,
            Timestamp = DateTime.Now
        };
        
        OnShow?.Invoke(toast);
    }
}

public class Toast
{
    public string Id { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DurationMs { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum ToastType
{
    Success,
    Error,
    Info,
    Warning
}
