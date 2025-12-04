using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class DemandPackageConfiguration : IEntityTypeConfiguration<DemandPackage>
{
    public void Configure(EntityTypeBuilder<DemandPackage> builder)
    {
        builder.ToTable("DemandPackages");

        builder.HasKey(x => x.PackageId);

        // Indexes for performance
        builder.HasIndex(x => x.SubrogationCaseId)
            .HasDatabaseName("IX_DemandPackages_SubrogationCaseId");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_DemandPackages_TenantId");

        builder.HasIndex(x => new { x.SubrogationCaseId, x.VersionNumber })
            .HasDatabaseName("IX_DemandPackages_CaseId_Version");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_DemandPackages_Status");

        // Property configurations
        builder.Property(x => x.MergedCoverLetterPath)
            .HasMaxLength(500);

        builder.Property(x => x.GeneratedPdfPath)
            .HasMaxLength(500);

        builder.Property(x => x.PdfHash)
            .HasMaxLength(64); // SHA256

        builder.Property(x => x.BookmarksJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships - explicitly configure to avoid lazy loading
        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey(d => d.DemandPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
