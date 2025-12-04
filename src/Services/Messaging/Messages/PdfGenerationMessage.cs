namespace SubrogationDemandManagement.Services.Messaging.Messages;

/// <summary>
/// Message sent to trigger PDF generation
/// </summary>
public class PdfGenerationMessage
{
    public Guid PackageId { get; set; }
    public Guid TenantId { get; set; }
    public Guid SubrogationCaseId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
