using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.TenantId);

        // Indexes for performance
        builder.HasIndex(x => x.TenantCode)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_TenantCode");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Tenants_IsActive");

        // Property configurations
        builder.Property(x => x.TenantName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.TenantCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SSOProvider)
            .HasMaxLength(50);

        builder.Property(x => x.SSOMetadataUrl)
            .HasMaxLength(500);

        builder.Property(x => x.SSOClientId)
            .HasMaxLength(100);

        builder.Property(x => x.ParentSystemApiUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ParentSystemApiKey)
            .HasMaxLength(500); // Encrypted

        builder.Property(x => x.EmailFromAddress)
            .HasMaxLength(255);

        builder.Property(x => x.EmailFromName)
            .HasMaxLength(255);

        builder.Property(x => x.FeaturesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SubscriptionTier)
            .IsRequired()
            .HasMaxLength(20);
    }
}
