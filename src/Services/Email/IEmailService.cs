namespace SubrogationDemandManagement.Services.Email;

/// <summary>
/// Interface for email sending operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email with optional attachments
    /// </summary>
    /// <param name="request">Email request details</param>
    /// <returns>Email response with message ID and status</returns>
    Task<EmailResponse> SendEmailAsync(EmailRequest request);
}

public class EmailRequest
{
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class EmailResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }
}
