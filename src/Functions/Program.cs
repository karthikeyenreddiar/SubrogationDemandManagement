using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Services.Data;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Storage;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Add Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();

        // Add DbContext
        services.AddDbContext<SubrogationDbContext>(options =>
        {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(30);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        // Register repositories
        services.AddScoped<ISubrogationCaseRepository, SubrogationCaseRepository>();
        services.AddScoped<SubrogationCaseRepository>();
        services.AddScoped<DemandPackageRepository>();

        // Register Azure services
        services.AddSingleton<BlobStorageService>();
    })
    .Build();

host.Run();
