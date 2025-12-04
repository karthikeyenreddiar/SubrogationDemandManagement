namespace SubrogationDemandManagement.Services.Auth;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? UserId { get; }
}
