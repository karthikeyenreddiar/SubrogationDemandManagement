using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Repositories;

public interface ISubrogationCaseRepository
{
    Task<List<SubrogationCase>> GetByTenantAndStatus(Guid tenantId, CaseStatus status);
    Task<SubrogationCase?> GetByIdAsync(Guid caseId);
    Task<SubrogationCase?> GetByIdForUpdateAsync(Guid caseId);
    Task<List<SubrogationCase>> GetByTenantAsync(Guid tenantId, int skip = 0, int take = 50);
    Task<SubrogationCase> CreateAsync(SubrogationCase subrogationCase);
    Task UpdateAsync(SubrogationCase subrogationCase);
    Task UpdatePropertiesAsync(Guid caseId, Action<SubrogationCase> updateAction);
    Task<int> GetCountByTenantAndStatusAsync(Guid tenantId, CaseStatus status);
}
