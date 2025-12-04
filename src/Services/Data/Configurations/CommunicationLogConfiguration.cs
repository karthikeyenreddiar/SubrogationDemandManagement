using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data.Configurations;

public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> builder)
    {
        builder.ToTable("CommunicationLogs");

        builder.HasKey(x => x.CommunicationId);

        // Indexes for performance
        builder.HasIndex(x => x.DemandPackageId)
            .HasDatabaseName("IX_CommunicationLogs_DemandPackageId");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_CommunicationLogs_TenantId");

        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("IX_CommunicationLogs_Status_CreatedAt");

        builder.HasIndex(x => x.DeliveryTrackingId)
            .HasDatabaseName("IX_CommunicationLogs_DeliveryTrackingId");

        // Property configurations
        builder.Property(x => x.RecipientsJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CcRecipientsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EmailSubject)
            .HasMaxLength(500);

        builder.Property(x => x.EmailBody)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.FromAddress)
            .HasMaxLength(255);

        builder.Property(x => x.DeliveryTrackingId)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(x => x.InitiatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
