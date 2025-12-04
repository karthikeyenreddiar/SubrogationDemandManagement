namespace SubrogationDemandManagement.Domain.Models;

public class Template
{
    public Guid TemplateId { get; set; }
    public Guid TenantId { get; set; }
    
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Classification
    public string? Jurisdiction { get; set; }
    public string? LineOfBusiness { get; set; }
    public string? LossType { get; set; }
    public TemplatePhase Phase { get; set; }
    
    // Storage
    public string BlobStoragePath { get; set; } = string.Empty;
    public TemplateFormat Format { get; set; }
    
    // Merge fields (JSON array)
    public string? MergeFields { get; set; }
    
    // Source
    public TemplateSource Source { get; set; }
    public string? ExternalCMSId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public enum TemplatePhase
{
    Initial,
    FollowUp,
    Arbitration,
    Final
}

public enum TemplateFormat
{
    DOCX,
    PDF
}

public enum TemplateSource
{
    CMS,
    LocalUpload
}
