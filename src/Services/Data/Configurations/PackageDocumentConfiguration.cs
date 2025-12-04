using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class PackageDocumentConfiguration : IEntityTypeConfiguration<PackageDocument>
{
    public void Configure(EntityTypeBuilder<PackageDocument> builder)
    {
        builder.ToTable("PackageDocuments");

        builder.HasKey(x => x.PackageDocumentId);

        // Indexes for performance
        builder.HasIndex(x => x.DemandPackageId)
            .HasDatabaseName("IX_PackageDocuments_DemandPackageId");

        builder.HasIndex(x => new { x.DemandPackageId, x.DisplayOrder })
            .HasDatabaseName("IX_PackageDocuments_PackageId_DisplayOrder");

        // Property configurations
        builder.Property(x => x.DocumentName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.BlobStoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ExternalDocumentId)
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
