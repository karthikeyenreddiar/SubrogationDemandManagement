using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(x => x.TemplateId);

        // Indexes for performance
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_Templates_TenantId");

        builder.HasIndex(x => new { x.TenantId, x.Jurisdiction, x.LineOfBusiness })
            .HasDatabaseName("IX_Templates_TenantId_Jurisdiction_LOB");

        builder.HasIndex(x => x.ExternalCMSId)
            .HasDatabaseName("IX_Templates_ExternalCMSId");

        // Property configurations
        builder.Property(x => x.TemplateName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Jurisdiction)
            .HasMaxLength(50);

        builder.Property(x => x.LineOfBusiness)
            .HasMaxLength(50);

        builder.Property(x => x.LossType)
            .HasMaxLength(50);

        builder.Property(x => x.BlobStoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.MergeFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ExternalCMSId)
            .HasMaxLength(100);

        builder.Property(x => x.Phase)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Format)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
