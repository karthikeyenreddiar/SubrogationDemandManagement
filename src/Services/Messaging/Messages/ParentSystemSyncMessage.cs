namespace SubrogationDemandManagement.Services.Messaging.Messages;

/// <summary>
/// Message sent to sync data back to parent claim system
/// </summary>
public class ParentSystemSyncMessage
{
    public Guid TenantId { get; set; }
    public Guid SubrogationCaseId { get; set; }
    public string ClaimId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // DemandSent, ResponseReceived, Settled
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime OccurredAt { get; set; }
}
