namespace SubrogationDemandManagement.Domain.Models;

public class PackageDocument
{
    public Guid PackageDocumentId { get; set; }
    public Guid DemandPackageId { get; set; }
    
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public DocumentSource Source { get; set; }
    
    public string BlobStoragePath { get; set; } = string.Empty;
    public string? ExternalDocumentId { get; set; } // From parent system
    
    public int DisplayOrder { get; set; }
    public bool IsIncluded { get; set; }
    public bool IsSensitive { get; set; } // Privacy flag
    
    public DateTime UploadedAt { get; set; }
}

public enum DocumentType
{
    PoliceReport,
    Estimate,
    Photo,
    MedicalBill,
    RepairInvoice,
    Correspondence,
    Other
}

public enum DocumentSource
{
    ClaimSystem,
    UserUpload
}
