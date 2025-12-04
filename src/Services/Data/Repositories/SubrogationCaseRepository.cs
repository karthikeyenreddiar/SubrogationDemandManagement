using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;
using System.Linq.Expressions;

namespace SubrogationDemandManagement.Services.Data.Repositories;

/// <summary>
/// Performance-focused repository for SubrogationCase with compiled queries
/// </summary>
public class SubrogationCaseRepository : ISubrogationCaseRepository
{
    private readonly SubrogationDbContext _context;

    public SubrogationCaseRepository(SubrogationDbContext context)
    {
        _context = context;
    }

    // Compiled query for frequently used tenant+status filter
    private static readonly Func<SubrogationDbContext, Guid, CaseStatus, Task<List<SubrogationCase>>> 
        GetCasesByTenantAndStatusQuery = EF.CompileAsyncQuery(
            (SubrogationDbContext context, Guid tenantId, CaseStatus status) =>
                context.SubrogationCases
                    .Where(c => c.TenantId == tenantId && c.Status == status)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList());

    // Compiled query for single case by ID (no tracking for read-only)
    private static readonly Func<SubrogationDbContext, Guid, Task<SubrogationCase?>>
        GetCaseByIdQuery = EF.CompileAsyncQuery(
            (SubrogationDbContext context, Guid caseId) =>
                context.SubrogationCases
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CaseId == caseId));

    /// <summary>
    /// Get cases by tenant and status using compiled query
    /// </summary>
    public Task<List<SubrogationCase>> GetByTenantAndStatus(Guid tenantId, CaseStatus status)
    {
        return GetCasesByTenantAndStatusQuery(_context, tenantId, status);
    }

    /// <summary>
    /// Get case by ID (no tracking for read-only scenarios)
    /// </summary>
    public Task<SubrogationCase?> GetByIdAsync(Guid caseId)
    {
        return GetCaseByIdQuery(_context, caseId);
    }

    /// <summary>
    /// Get case by ID with tracking for updates
    /// </summary>
    public async Task<SubrogationCase?> GetByIdForUpdateAsync(Guid caseId)
    {
        return await _context.SubrogationCases
            .AsTracking() // Explicitly enable tracking for updates
            .FirstOrDefaultAsync(c => c.CaseId == caseId);
    }

    /// <summary>
    /// Get cases by tenant with pagination (no tracking)
    /// </summary>
    public async Task<List<SubrogationCase>> GetByTenantAsync(
        Guid tenantId, 
        int skip = 0, 
        int take = 50)
    {
        return await _context.SubrogationCases
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Create new case
    /// </summary>
    public async Task<SubrogationCase> CreateAsync(SubrogationCase subrogationCase)
    {
        _context.SubrogationCases.Add(subrogationCase);
        await _context.SaveChangesAsync();
        return subrogationCase;
    }

    /// <summary>
    /// Update existing case
    /// </summary>
    public async Task UpdateAsync(SubrogationCase subrogationCase)
    {
        // Attach and mark as modified
        _context.SubrogationCases.Attach(subrogationCase);
        _context.Entry(subrogationCase).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Partial update - only update specific properties
    /// </summary>
    public async Task UpdatePropertiesAsync(
        Guid caseId, 
        Action<SubrogationCase> updateAction)
    {
        var caseEntity = await GetByIdForUpdateAsync(caseId);
        if (caseEntity == null)
            throw new InvalidOperationException($"Case {caseId} not found");

        updateAction(caseEntity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get count by tenant and status (optimized query)
    /// </summary>
    public async Task<int> GetCountByTenantAndStatusAsync(Guid tenantId, CaseStatus status)
    {
        return await _context.SubrogationCases
            .AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status == status);
    }
}
