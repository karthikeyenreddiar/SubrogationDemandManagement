using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data;

namespace SubrogationDemandManagement.API.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SubrogationDbContext context)
    {
        // Check if data already exists
        if (context.SubrogationCases.Any())
        {
            return;
        }

        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Create Tenant
        var tenant = new Tenant
        {
            TenantId = tenantId,
            TenantName = "Acme Insurance",
            TenantCode = "ACME",
            IsActive = true,
            SubscriptionTier = "Standard",
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);

        // Create Cases
        var cases = new List<SubrogationCase>
        {
            new SubrogationCase
            {
                CaseId = Guid.NewGuid(),
                TenantId = tenantId,
                ClaimId = "CLM-2023-001",
                PolicyNumber = "POL-123456789",
                LossDate = DateTime.UtcNow.AddDays(-30),
                InsuredLiabilityPercent = 0,
                ThirdPartyLiabilityPercent = 100,
                TotalPaidIndemnity = 5000.00m,
                TotalPaidExpense = 500.00m,
                OutstandingReserves = 0,
                RecoverySought = 5500.00m,
                Status = CaseStatus.Draft,
                InternalNotes = "Clear liability, rear-end collision.",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new SubrogationCase
            {
                CaseId = Guid.NewGuid(),
                TenantId = tenantId,
                ClaimId = "CLM-2023-002",
                PolicyNumber = "POL-987654321",
                LossDate = DateTime.UtcNow.AddDays(-60),
                InsuredLiabilityPercent = 20,
                ThirdPartyLiabilityPercent = 80,
                TotalPaidIndemnity = 12000.00m,
                TotalPaidExpense = 1500.00m,
                OutstandingReserves = 5000.00m,
                RecoverySought = 10800.00m, // 80% of (12000 + 1500)
                Status = CaseStatus.DemandSent,
                InternalNotes = "Shared liability, intersection accident.",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new SubrogationCase
            {
                CaseId = Guid.NewGuid(),
                TenantId = tenantId,
                ClaimId = "CLM-2023-003",
                PolicyNumber = "POL-456123789",
                LossDate = DateTime.UtcNow.AddDays(-10),
                InsuredLiabilityPercent = 0,
                ThirdPartyLiabilityPercent = 100,
                TotalPaidIndemnity = 2500.00m,
                TotalPaidExpense = 0,
                OutstandingReserves = 10000.00m,
                RecoverySought = 2500.00m,
                Status = CaseStatus.Draft,
                InternalNotes = "Waiting for police report.",
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
        context.SubrogationCases.AddRange(cases);

        // Create Packages for Case 2
        var case2 = cases[1];
        var package = new DemandPackage
        {
            PackageId = Guid.NewGuid(),
            SubrogationCaseId = case2.CaseId,
            TenantId = tenantId,
            VersionNumber = 1,
            Status = PackageStatus.Generated,
            GeneratedPdfPath = $"{tenantId}/{case2.CaseId}/package_v1.pdf",
            PageCount = 15,
            PdfSizeBytes = 2048576,
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow.AddDays(-50)
        };
        context.DemandPackages.Add(package);

        await context.SaveChangesAsync();
    }
}
