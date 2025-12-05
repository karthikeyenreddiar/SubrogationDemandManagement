using Xunit;
using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data;
using SubrogationDemandManagement.Services.Data.Repositories;

namespace SubrogationDemandManagement.Tests.Repositories;

public class CommunicationLogRepositoryTests : IDisposable
{
    private readonly SubrogationDbContext _context;
    private readonly CommunicationLogRepository _repository;

    public CommunicationLogRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<SubrogationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SubrogationDbContext(options);
        _repository = new CommunicationLogRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_WithValidLog_CreatesSuccessfully()
    {
        // Arrange
        var log = new CommunicationLog
        {
            CommunicationId = Guid.NewGuid(),
            DemandPackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = "[\"test@example.com\"]",
            EmailSubject = "Test Subject",
            EmailBody = "Test Body",
            FromAddress = "sender@example.com",
            Status = CommunicationStatus.Queued,
            InitiatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(log);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(log.CommunicationId, result.CommunicationId);
        
        var retrieved = await _context.CommunicationLogs.FindAsync(log.CommunicationId);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Subject", retrieved.EmailSubject);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsLog()
    {
        // Arrange
        var log = new CommunicationLog
        {
            CommunicationId = Guid.NewGuid(),
            DemandPackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = "[\"test@example.com\"]",
            Status = CommunicationStatus.Queued,
            InitiatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        await _context.CommunicationLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(log.CommunicationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(log.CommunicationId, result.CommunicationId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPackageIdAsync_WithMultipleLogs_ReturnsAllForPackage()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var otherPackageId = Guid.NewGuid();

        var logs = new[]
        {
            new CommunicationLog
            {
                CommunicationId = Guid.NewGuid(),
                DemandPackageId = packageId,
                TenantId = Guid.NewGuid(),
                Action = CommunicationAction.InitialDemand,
                Channel = CommunicationChannel.Email,
                RecipientsJson = "[]",
                Status = CommunicationStatus.Sent,
                InitiatedBy = "User1",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new CommunicationLog
            {
                CommunicationId = Guid.NewGuid(),
                DemandPackageId = packageId,
                TenantId = Guid.NewGuid(),
                Action = CommunicationAction.FollowUp,
                Channel = CommunicationChannel.Email,
                RecipientsJson = "[]",
                Status = CommunicationStatus.Sent,
                InitiatedBy = "User1",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new CommunicationLog
            {
                CommunicationId = Guid.NewGuid(),
                DemandPackageId = otherPackageId,
                TenantId = Guid.NewGuid(),
                Action = CommunicationAction.InitialDemand,
                Channel = CommunicationChannel.Email,
                RecipientsJson = "[]",
                Status = CommunicationStatus.Sent,
                InitiatedBy = "User2",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.CommunicationLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPackageIdAsync(packageId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, log => Assert.Equal(packageId, log.DemandPackageId));
        // Verify ordering (most recent first)
        Assert.True(result[0].CreatedAt > result[1].CreatedAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToSent_UpdatesStatusAndSentAt()
    {
        // Arrange
        var log = new CommunicationLog
        {
            CommunicationId = Guid.NewGuid(),
            DemandPackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = "[]",
            Status = CommunicationStatus.Queued,
            InitiatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        await _context.CommunicationLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        await _repository.UpdateStatusAsync(log.CommunicationId, CommunicationStatus.Sent, "msg-123");

        // Assert
        var updated = await _context.CommunicationLogs.FindAsync(log.CommunicationId);
        Assert.NotNull(updated);
        Assert.Equal(CommunicationStatus.Sent, updated.Status);
        Assert.Equal("msg-123", updated.DeliveryTrackingId);
        Assert.NotNull(updated.SentAt);
        Assert.True(updated.SentAt >= beforeUpdate);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToDelivered_UpdatesStatusAndDeliveredAt()
    {
        // Arrange
        var log = new CommunicationLog
        {
            CommunicationId = Guid.NewGuid(),
            DemandPackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = "[]",
            Status = CommunicationStatus.Sent,
            InitiatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow
        };

        await _context.CommunicationLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        await _repository.UpdateStatusAsync(log.CommunicationId, CommunicationStatus.Delivered);

        // Assert
        var updated = await _context.CommunicationLogs.FindAsync(log.CommunicationId);
        Assert.NotNull(updated);
        Assert.Equal(CommunicationStatus.Delivered, updated.Status);
        Assert.NotNull(updated.DeliveredAt);
        Assert.True(updated.DeliveredAt >= beforeUpdate);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithNonExistingId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.UpdateStatusAsync(Guid.NewGuid(), CommunicationStatus.Sent));
    }

    [Fact]
    public async Task UpdateErrorAsync_WithValidId_UpdatesStatusAndErrorMessage()
    {
        // Arrange
        var log = new CommunicationLog
        {
            CommunicationId = Guid.NewGuid(),
            DemandPackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = "[]",
            Status = CommunicationStatus.Sending,
            InitiatedBy = "TestUser",
            CreatedAt = DateTime.UtcNow
        };

        await _context.CommunicationLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateErrorAsync(log.CommunicationId, "Email delivery failed");

        // Assert
        var updated = await _context.CommunicationLogs.FindAsync(log.CommunicationId);
        Assert.NotNull(updated);
        Assert.Equal(CommunicationStatus.Failed, updated.Status);
        Assert.Equal("Email delivery failed", updated.ErrorMessage);
    }

    [Fact]
    public async Task UpdateErrorAsync_WithNonExistingId_DoesNotThrow()
    {
        // Act & Assert - should not throw
        await _repository.UpdateErrorAsync(Guid.NewGuid(), "Error message");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
