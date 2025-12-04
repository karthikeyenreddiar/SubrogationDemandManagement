using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class SubrogationCaseConfiguration : IEntityTypeConfiguration<SubrogationCase>
{
    public void Configure(EntityTypeBuilder<SubrogationCase> builder)
    {
        builder.ToTable("SubrogationCases");

        builder.HasKey(x => x.CaseId);

        // Indexes for performance
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_SubrogationCases_TenantId");

        builder.HasIndex(x => x.ClaimId)
            .HasDatabaseName("IX_SubrogationCases_ClaimId");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("IX_SubrogationCases_TenantId_Status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_SubrogationCases_CreatedAt");

        // Property configurations
        builder.Property(x => x.ClaimId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.InsuredLiabilityPercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.ThirdPartyLiabilityPercent)
            .HasPrecision(5, 2);

        builder.Property(x => x.TotalPaidIndemnity)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalPaidExpense)
            .HasPrecision(18, 2);

        builder.Property(x => x.OutstandingReserves)
            .HasPrecision(18, 2);

        builder.Property(x => x.RecoverySought)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaymentBreakdown)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
