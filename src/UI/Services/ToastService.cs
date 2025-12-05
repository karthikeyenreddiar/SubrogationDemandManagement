using System;

namespace SubrogationDemandManagement.UI.Services;

public class ToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<ToastMessage>? OnHide;

    public void ShowToast(string message, ToastLevel level = ToastLevel.Info, string title = "", int duration = 5000)
    {
        OnShow?.Invoke(new ToastMessage 
        { 
            Message = message, 
            Level = level, 
            Title = title,
            Duration = duration,
            Id = Guid.NewGuid()
        });
    }

    public void ShowSuccess(string message, string title = "Success") => ShowToast(message, ToastLevel.Success, title);
    public void ShowError(string message, string title = "Error") => ShowToast(message, ToastLevel.Error, title);
    public void ShowInfo(string message, string title = "Info") => ShowToast(message, ToastLevel.Info, title);
    public void ShowWarning(string message, string title = "Warning") => ShowToast(message, ToastLevel.Warning, title);
}

public class ToastMessage
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ToastLevel Level { get; set; }
    public int Duration { get; set; } = 5000;
}

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}
