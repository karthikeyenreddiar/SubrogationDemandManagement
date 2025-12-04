using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data;
using SubrogationDemandManagement.Services.Data.Repositories;

namespace SubrogationDemandManagement.Tests;

public class SubrogationCaseRepositoryTests
{
    private readonly DbContextOptions<SubrogationDbContext> _options;

    public SubrogationCaseRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<SubrogationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private async Task<SubrogationDbContext> GetContextAsync()
    {
        var context = new SubrogationDbContext(_options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCaseToDatabase()
    {
        // Arrange
        using var context = await GetContextAsync();
        var repository = new SubrogationCaseRepository(context);
        var subrogationCase = new SubrogationCase
        {
            CaseId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            ClaimId = "TEST-001",
            Status = CaseStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await repository.CreateAsync(subrogationCase);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(subrogationCase.CaseId, result.CaseId);

        using var verifyContext = await GetContextAsync();
        var savedCase = await verifyContext.SubrogationCases.FirstOrDefaultAsync(c => c.CaseId == subrogationCase.CaseId);
        Assert.NotNull(savedCase);
        Assert.Equal("TEST-001", savedCase.ClaimId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCase_WhenExists()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        using (var context = await GetContextAsync())
        {
            context.SubrogationCases.Add(new SubrogationCase
            {
                CaseId = caseId,
                TenantId = Guid.NewGuid(),
                ClaimId = "TEST-002",
                Status = CaseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        using (var context = await GetContextAsync())
        {
            var repository = new SubrogationCaseRepository(context);

            // Act
            var result = await repository.GetByIdAsync(caseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(caseId, result.CaseId);
            Assert.Equal("TEST-002", result.ClaimId);
        }
    }

    [Fact]
    public async Task GetByTenantAsync_ShouldReturnMatchingCases()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        using (var context = await GetContextAsync())
        {
            context.SubrogationCases.AddRange(
                new SubrogationCase
                {
                    CaseId = Guid.NewGuid(),
                    TenantId = tenantId,
                    Status = CaseStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                },
                new SubrogationCase
                {
                    CaseId = Guid.NewGuid(),
                    TenantId = tenantId,
                    Status = CaseStatus.DemandSent,
                    CreatedAt = DateTime.UtcNow
                },
                new SubrogationCase
                {
                    CaseId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(), // Different tenant
                    Status = CaseStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }

        using (var context = await GetContextAsync())
        {
            var repository = new SubrogationCaseRepository(context);

            // Act
            var result = await repository.GetByTenantAsync(tenantId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(tenantId, c.TenantId));
        }
    }
}
