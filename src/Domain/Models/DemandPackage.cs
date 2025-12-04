namespace SubrogationDemandManagement.Domain.Models;

public class DemandPackage
{
    public Guid PackageId { get; set; }
    public Guid SubrogationCaseId { get; set; }
    public Guid TenantId { get; set; }
    
    // Template
    public Guid? TemplateId { get; set; }
    public string? MergedCoverLetterPath { get; set; }
    
    // Documents (navigation property)
    public List<PackageDocument> Documents { get; set; } = new();
    
    // Generated PDF
    public string? GeneratedPdfPath { get; set; }
    public string? PdfHash { get; set; } // SHA256 for audit
    public long PdfSizeBytes { get; set; }
    
    // Metadata
    public int PageCount { get; set; }
    public string? BookmarksJson { get; set; } // JSON array of bookmarks
    
    // Version
    public int VersionNumber { get; set; }
    public PackageStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public enum PackageStatus
{
    Draft,
    Generating,
    Generated,
    Sent,
    Failed
}
