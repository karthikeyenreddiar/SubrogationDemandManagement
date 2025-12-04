using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Services.Data;

public class SubrogationDbContext : DbContext
{
    public SubrogationDbContext(DbContextOptions<SubrogationDbContext> options)
        : base(options)
    {
        // Disable change tracking by default for read-only queries
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        
        // Disable lazy loading to prevent N+1 queries
        ChangeTracker.LazyLoadingEnabled = false;
        
        // Disable automatic detection of changes for better performance
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public DbSet<SubrogationCase> SubrogationCases => Set<SubrogationCase>();
    public DbSet<DemandPackage> DemandPackages => Set<DemandPackage>();
    public DbSet<PackageDocument> PackageDocuments => Set<PackageDocument>();
    public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubrogationDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging only in development
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
    }
}
