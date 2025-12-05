using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Repositories;

public class CommunicationLogRepository
{
    private readonly SubrogationDbContext _context;

    public CommunicationLogRepository(SubrogationDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationLog?> GetByIdAsync(Guid id)
    {
        return await _context.CommunicationLogs.FindAsync(id);
    }

    public async Task<CommunicationLog> CreateAsync(CommunicationLog log)
    {
        _context.CommunicationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<List<CommunicationLog>> GetByPackageIdAsync(Guid packageId)
    {
        return await _context.CommunicationLogs
            .Where(c => c.DemandPackageId == packageId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(Guid id, CommunicationStatus status, string? externalId = null)
    {
        var log = await _context.CommunicationLogs.FindAsync(id);
        if (log == null)
        {
            throw new InvalidOperationException($"Communication log with ID {id} not found");
        }

        log.Status = status;
        if (externalId != null)
        {
            log.DeliveryTrackingId = externalId;
        }
        
        if (status == CommunicationStatus.Sent)
        {
            log.SentAt = DateTime.UtcNow;
        }
        else if (status == CommunicationStatus.Delivered)
        {
            log.DeliveredAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateErrorAsync(Guid id, string errorMessage)
    {
        var log = await _context.CommunicationLogs.FindAsync(id);
        if (log != null)
        {
            log.Status = CommunicationStatus.Failed;
            log.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
    }
}
