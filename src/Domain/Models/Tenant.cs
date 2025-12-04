namespace SubrogationDemandManagement.Domain.Models;

public class Tenant
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantCode { get; set; } = string.Empty;
    
    // SSO Configuration
    public string? SSOProvider { get; set; } // AzureAD, SAML, OIDC
    public string? SSOMetadataUrl { get; set; }
    public string? SSOClientId { get; set; }
    
    // Parent System Integration
    public string? ParentSystemApiUrl { get; set; }
    public string? ParentSystemApiKey { get; set; } // Encrypted
    
    // Email Configuration
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }
    
    // Features
    public string? FeaturesJson { get; set; } // JSON object of enabled features
    
    // Subscription
    public string SubscriptionTier { get; set; } = "Basic"; // Basic, Standard, Premium
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
