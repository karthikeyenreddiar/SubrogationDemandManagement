using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Repositories;

/// <summary>
/// Performance-focused repository for DemandPackage with explicit loading
/// </summary>
public class DemandPackageRepository
{
    private readonly SubrogationDbContext _context;

    public DemandPackageRepository(SubrogationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get package by ID without documents (lightweight)
    /// </summary>
    public virtual async Task<DemandPackage?> GetByIdAsync(Guid packageId)
    {
        return await _context.DemandPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PackageId == packageId);
    }

    /// <summary>
    /// Get package with documents using explicit loading (performance optimized)
    /// </summary>
    public virtual async Task<DemandPackage?> GetByIdWithDocumentsAsync(Guid packageId)
    {
        var package = await _context.DemandPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PackageId == packageId);

        if (package != null)
        {
            // Explicitly load documents in a separate query (split query)
            var documents = await _context.PackageDocuments
                .AsNoTracking()
                .Where(d => d.DemandPackageId == packageId)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            package.Documents = documents;
        }

        return package;
    }

    /// <summary>
    /// Get packages by case ID (no tracking, no documents)
    /// </summary>
    public virtual async Task<List<DemandPackage>> GetByCaseIdAsync(Guid caseId)
    {
        return await _context.DemandPackages
            .AsNoTracking()
            .Where(p => p.SubrogationCaseId == caseId)
            .OrderByDescending(p => p.VersionNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Get latest package version for a case
    /// </summary>
    public virtual async Task<DemandPackage?> GetLatestByCaseIdAsync(Guid caseId)
    {
        return await _context.DemandPackages
            .AsNoTracking()
            .Where(p => p.SubrogationCaseId == caseId)
            .OrderByDescending(p => p.VersionNumber)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create new package
    /// </summary>
    public virtual async Task<DemandPackage> CreateAsync(DemandPackage package)
    {
        _context.DemandPackages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    /// <summary>
    /// Update package status (optimized - only updates status field)
    /// </summary>
    public virtual async Task UpdateStatusAsync(Guid packageId, PackageStatus status)
    {
        await _context.DemandPackages
            .Where(p => p.PackageId == packageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Status, status));
    }

    /// <summary>
    /// Update package with generated PDF info
    /// </summary>
    public virtual async Task UpdateGeneratedPdfAsync(
        Guid packageId, 
        string pdfPath, 
        string pdfHash, 
        long sizeBytes, 
        int pageCount)
    {
        await _context.DemandPackages
            .Where(p => p.PackageId == packageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.GeneratedPdfPath, pdfPath)
                .SetProperty(p => p.PdfHash, pdfHash)
                .SetProperty(p => p.PdfSizeBytes, sizeBytes)
                .SetProperty(p => p.PageCount, pageCount)
                .SetProperty(p => p.Status, PackageStatus.Generated));
    }

    /// <summary>
    /// Add document to package
    /// </summary>
    public virtual async Task AddDocumentAsync(PackageDocument document)
    {
        _context.PackageDocuments.Add(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete document from package
    /// </summary>
    public virtual async Task DeleteDocumentAsync(Guid documentId)
    {
        await _context.PackageDocuments
            .Where(d => d.PackageDocumentId == documentId)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Update document display order (batch update)
    /// </summary>
    public virtual async Task UpdateDocumentOrderAsync(Dictionary<Guid, int> documentOrders)
    {
        foreach (var (documentId, order) in documentOrders)
        {
            await _context.PackageDocuments
                .Where(d => d.PackageDocumentId == documentId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.DisplayOrder, order));
        }
    }

    /// <summary>
    /// Get packages by status for monitoring (lightweight projection)
    /// </summary>
    public virtual async Task<List<(Guid PackageId, Guid CaseId, PackageStatus Status, DateTime CreatedAt)>> 
        GetByStatusAsync(PackageStatus status, int limit = 100)
    {
        return await _context.DemandPackages
            .AsNoTracking()
            .Where(p => p.Status == status)
            .OrderBy(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ValueTuple<Guid, Guid, PackageStatus, DateTime>(
                p.PackageId, 
                p.SubrogationCaseId, 
                p.Status, 
                p.CreatedAt))
            .ToListAsync();
    }
}
