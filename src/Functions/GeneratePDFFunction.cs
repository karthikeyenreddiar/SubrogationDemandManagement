using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Storage;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Functions.Services;
using QuestPDF.Fluent;
using System.Text.Json;

namespace SubrogationDemandManagement.Functions;

public class GeneratePDFFunction
{
    private readonly ILogger<GeneratePDFFunction> _logger;
    private readonly DemandPackageRepository _packageRepository;
    private readonly ISubrogationCaseRepository _caseRepository;
    private readonly BlobStorageService _blobStorage;

    public GeneratePDFFunction(
        ILogger<GeneratePDFFunction> logger,
        DemandPackageRepository packageRepository,
        ISubrogationCaseRepository caseRepository,
        BlobStorageService blobStorage)
    {
        _logger = logger;
        _packageRepository = packageRepository;
        _caseRepository = caseRepository;
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

            // Get case details
            var subrogationCase = await _caseRepository.GetByIdAsync(package.SubrogationCaseId);
            if (subrogationCase == null)
            {
                _logger.LogError("Case {CaseId} not found", package.SubrogationCaseId);
                throw new InvalidOperationException($"Case {package.SubrogationCaseId} not found");
            }

            // Generate PDF using QuestPDF
            _logger.LogInformation("Generating PDF document...");
            var document = new DemandPackageDocument(subrogationCase, package);
            var pdfBytes = document.GeneratePdf();
            
            using var pdfStream = new MemoryStream(pdfBytes);
            
            // Upload generated PDF to Blob Storage
            var pdfPath = await _blobStorage.UploadPackageAsync(
                message.TenantId, 
                message.PackageId, 
                pdfStream);
            
            // Calculate hash for audit
            var pdfHash = CalculateHash(pdfBytes);
            
            // Update package with generated PDF info
            await _packageRepository.UpdateGeneratedPdfAsync(
                message.PackageId,
                pdfPath,
                pdfHash,
                pdfBytes.Length,
                1); // TODO: Get actual page count if needed
            
            var duration = DateTime.UtcNow - startTime;
            var memoryUsed = (GC.GetTotalMemory(false) - startMemory) / (1024 * 1024); // MB
            
            _logger.LogInformation(
                "PDF generation completed for package {PackageId} in {Duration}ms, Memory: {Memory}MB",
                message.PackageId, duration.TotalMilliseconds, memoryUsed);
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
