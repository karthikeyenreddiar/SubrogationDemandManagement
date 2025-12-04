using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Storage;
using SubrogationDemandManagement.Domain.Models;
using System.Text.Json;

namespace SubrogationDemandManagement.Functions;

public class GeneratePDFFunction
{
    private readonly ILogger<GeneratePDFFunction> _logger;
    private readonly DemandPackageRepository _packageRepository;
    private readonly BlobStorageService _blobStorage;

    public GeneratePDFFunction(
        ILogger<GeneratePDFFunction> logger,
        DemandPackageRepository packageRepository,
        BlobStorageService blobStorage)
    {
        _logger = logger;
        _packageRepository = packageRepository;
        _blobStorage = blobStorage;
    }

    /// <summary>
    /// Azure Function triggered by Service Bus queue to generate PDF packages
    /// </summary>
    [Function("GeneratePDF")]
    public async Task Run(
        [ServiceBusTrigger("pdf-generation", Connection = "ServiceBusConnection")] 
        string messageBody)
    {
        var startTime = DateTime.UtcNow;
        var startMemory = GC.GetTotalMemory(false);
        
        _logger.LogInformation("PDF generation started. Message: {Message}", messageBody);
        
        try
        {
            // Deserialize message
            var message = JsonSerializer.Deserialize<PdfGenerationMessage>(messageBody);
            if (message == null)
            {
                _logger.LogError("Failed to deserialize PDF generation message");
                throw new InvalidOperationException("Invalid message format");
            }

            _logger.LogInformation("Processing PDF generation for package {PackageId}", message.PackageId);
            
            // Get package with documents
            var package = await _packageRepository.GetByIdWithDocumentsAsync(message.PackageId);
            if (package == null)
            {
                _logger.LogError("Package {PackageId} not found", message.PackageId);
                throw new InvalidOperationException($"Package {message.PackageId} not found");
            }

            // TODO: Implement actual PDF generation logic
            // 1. Download cover letter template from Blob Storage
            // 2. Download all documents from Blob Storage
            // 3. Merge into single PDF with bookmarks
            // 4. Add headers/footers
            // 5. Compress PDF
            
            // For now, simulate PDF generation
            await Task.Delay(500); // Simulate work
            
            // Create a simple PDF stream (placeholder)
            using var pdfStream = new MemoryStream();
            var pdfContent = System.Text.Encoding.UTF8.GetBytes($"PDF Package for {message.PackageId}");
            await pdfStream.WriteAsync(pdfContent);
            pdfStream.Position = 0;
            
            // Upload generated PDF to Blob Storage
            var pdfPath = await _blobStorage.UploadPackageAsync(
                message.TenantId, 
                message.PackageId, 
                pdfStream);
            
            // Calculate hash for audit
            var pdfHash = CalculateHash(pdfContent);
            
            // Update package with generated PDF info
            await _packageRepository.UpdateGeneratedPdfAsync(
                message.PackageId,
                pdfPath,
                pdfHash,
                pdfContent.Length,
                1); // Page count
            
            var duration = DateTime.UtcNow - startTime;
            var memoryUsed = (GC.GetTotalMemory(false) - startMemory) / (1024 * 1024); // MB
            
            _logger.LogInformation(
                "PDF generation completed for package {PackageId} in {Duration}ms, Memory: {Memory}MB",
                message.PackageId, duration.TotalMilliseconds, memoryUsed);
            
            // TODO: Track cost metrics
            // await _costTracker.TrackExecution("GeneratePDF", duration, memoryUsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed for message: {Message}", messageBody);
            throw; // Will trigger Service Bus retry
        }
    }

    private string CalculateHash(byte[] data)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
