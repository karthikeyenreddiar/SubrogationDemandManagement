namespace SubrogationDemandManagement.Services.Messaging.Messages;

/// <summary>
/// Message sent to trigger email delivery
/// </summary>
public class EmailDeliveryMessage
{
    public Guid CommunicationId { get; set; }
    public Guid PackageId { get; set; }
    public Guid TenantId { get; set; }
    public List<string> Recipients { get; set; } = new();
    public List<string>? CcRecipients { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PdfBlobPath { get; set; }
    public DateTime RequestedAt { get; set; }
}
