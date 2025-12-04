namespace SubrogationDemandManagement.Domain.Models;

public class SubrogationCase
{
    public Guid CaseId { get; set; }
    public Guid TenantId { get; set; }
    public string ClaimId { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime LossDate { get; set; }
    
    // Liability
    public decimal InsuredLiabilityPercent { get; set; }
    public decimal ThirdPartyLiabilityPercent { get; set; }
    
    // Damages
    public decimal TotalPaidIndemnity { get; set; }
    public decimal TotalPaidExpense { get; set; }
    public decimal OutstandingReserves { get; set; }
    public decimal RecoverySought { get; set; }
    
    // Payments (JSON array of included/excluded items)
    public string PaymentBreakdown { get; set; } = string.Empty;
    
    // Status
    public CaseStatus Status { get; set; }
    public string InternalNotes { get; set; } = string.Empty;
    
    // Audit
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public enum CaseStatus
{
    Draft,
    DemandSent,
    ResponseReceived,
    Negotiating,
    Settled,
    Closed,
    Cancelled
}
