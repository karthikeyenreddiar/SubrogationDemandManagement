namespace SubrogationDemandManagement.Domain.Models;

public class CommunicationLog
{
    public Guid CommunicationId { get; set; }
    public Guid DemandPackageId { get; set; }
    public Guid TenantId { get; set; }
    
    // Action
    public CommunicationAction Action { get; set; }
    public CommunicationChannel Channel { get; set; }
    
    // Recipients
    public string RecipientsJson { get; set; } = string.Empty; // JSON array
    public string? CcRecipientsJson { get; set; }
    
    // Email details
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public string? FromAddress { get; set; }
    
    // Delivery
    public CommunicationStatus Status { get; set; }
    public string? DeliveryTrackingId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Audit
    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public enum CommunicationAction
{
    InitialDemand,
    FollowUp,
    Response,
    FinalDemand
}

public enum CommunicationChannel
{
    Email,
    Print,
    Portal
}

public enum CommunicationStatus
{
    Queued,
    Sending,
    Sent,
    Delivered,
    Failed,
    Bounced
}
