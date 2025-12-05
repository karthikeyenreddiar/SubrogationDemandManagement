using Microsoft.EntityFrameworkCore;
using SubrogationDemandManagement.Services.Data;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Messaging;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add DbContext with performance optimizations
builder.Services.AddDbContext<SubrogationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });
});

// Register repositories
builder.Services.AddScoped<ISubrogationCaseRepository, SubrogationCaseRepository>();
builder.Services.AddScoped<SubrogationCaseRepository>(); // Keep concrete for Functions if needed or remove
builder.Services.AddScoped<DemandPackageRepository>();

// Register Service Bus service
builder.Services.AddSingleton<ServiceBusService>();

// Register Auth services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SubrogationDemandManagement.Services.Auth.ICurrentTenantService, SubrogationDemandManagement.API.Services.CurrentTenantService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy => policy
            .WithOrigins("https://localhost:7100", "http://localhost:5100")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SubrogationDbContext>();
        // Ensure database is created
        context.Database.EnsureCreated();
        await SubrogationDemandManagement.API.Data.DataSeeder.SeedAsync(context);
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
