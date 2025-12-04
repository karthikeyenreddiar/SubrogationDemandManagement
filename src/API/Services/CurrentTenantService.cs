using System.Security.Claims;
using SubrogationDemandManagement.Services.Auth;

namespace SubrogationDemandManagement.API.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            // Try to get TenantId from claims. 
            // This depends on how B2C is configured. 
            // Common patterns: "extension_TenantId", "tid", or a custom claim.
            // For this MVP, we'll look for a claim named "extension_TenantId" or "TenantId".
            
            var tenantClaim = user.FindFirst("extension_TenantId") ?? user.FindFirst("TenantId");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                return tenantId;
            }

            // Fallback for development/testing if needed, or return null
            return null;
        }
    }

    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            // "sub" is the standard subject claim for the user ID
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? user?.FindFirst("sub")?.Value;
        }
    }
}
