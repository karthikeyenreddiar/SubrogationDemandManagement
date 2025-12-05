using Microsoft.AspNetCore.Mvc;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Messaging;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Services.Storage;

namespace SubrogationDemandManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemandPackagesController : ControllerBase
{
    private readonly ILogger<DemandPackagesController> _logger;
    private readonly DemandPackageRepository _repository;
    private readonly CommunicationLogRepository _communicationRepository;
    private readonly ServiceBusService _serviceBus;
    private readonly BlobStorageService _blobStorage;

    public DemandPackagesController(
        ILogger<DemandPackagesController> logger,
        DemandPackageRepository repository,
        CommunicationLogRepository communicationRepository,
        ServiceBusService serviceBus,
        BlobStorageService blobStorage)
    {
        _logger = logger;
        _repository = repository;
        _communicationRepository = communicationRepository;
        _serviceBus = serviceBus;
        _blobStorage = blobStorage;
    }

    /// <summary>
    /// Get all demand packages for a case
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DemandPackage>>> GetPackages([FromQuery] Guid caseId)
    {
        _logger.LogInformation("Fetching packages for case {CaseId}", caseId);
        
        var packages = await _repository.GetByCaseIdAsync(caseId);
        return Ok(packages);
    }

    /// <summary>
    /// Get a specific demand package
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DemandPackage>> GetPackage(Guid id, [FromQuery] bool includeDocuments = false)
    {
        _logger.LogInformation("Fetching package {PackageId}, includeDocuments={IncludeDocuments}", 
            id, includeDocuments);
        
        var package = includeDocuments 
            ? await _repository.GetByIdWithDocumentsAsync(id)
            : await _repository.GetByIdAsync(id);
        
        if (package == null)
            return NotFound();
            
        return Ok(package);
    }

    /// <summary>
    /// Create a new demand package
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DemandPackage>> CreatePackage([FromBody] DemandPackage package)
    {
        _logger.LogInformation("Creating new package for case {CaseId}", package.SubrogationCaseId);
        
        package.PackageId = Guid.NewGuid();
        package.CreatedAt = DateTime.UtcNow;
        package.Status = PackageStatus.Draft;
        
        // Get latest version number for this case
        var latestPackage = await _repository.GetLatestByCaseIdAsync(package.SubrogationCaseId);
        package.VersionNumber = (latestPackage?.VersionNumber ?? 0) + 1;
        
        var created = await _repository.CreateAsync(package);
        
        return CreatedAtAction(nameof(GetPackage), new { id = created.PackageId }, created);
    }

    /// <summary>
    /// Queue PDF generation for a package (sends to Azure Functions via Service Bus)
    /// </summary>
    [HttpPost("{id}/generate")]
    public async Task<ActionResult> QueuePdfGeneration(Guid id)
    {
        _logger.LogInformation("Queueing PDF generation for package {PackageId}", id);
        
        // Get package details
        var package = await _repository.GetByIdAsync(id);
        if (package == null)
            return NotFound();
        
        // Update status to Generating
        await _repository.UpdateStatusAsync(id, PackageStatus.Generating);
        
        // Send message to Service Bus queue 'pdf-generation'
        var message = new PdfGenerationMessage
        {
            PackageId = package.PackageId,
            TenantId = package.TenantId,
            SubrogationCaseId = package.SubrogationCaseId,
            RequestedBy = User.Identity?.Name ?? "System",
            RequestedAt = DateTime.UtcNow
        };
        
        await _serviceBus.SendMessageAsync("pdf-generation", message);
        
        _logger.LogInformation("PDF generation queued for package {PackageId}", id);
        
        return Accepted(new { message = "PDF generation queued", packageId = id, status = PackageStatus.Generating });
    }

    /// <summary>
    /// Send demand package via email (queues to Azure Functions)
    /// </summary>
    [HttpPost("{id}/send")]
    public async Task<ActionResult> SendPackage(Guid id, [FromBody] SendPackageRequest request)
    {
        _logger.LogInformation("Queueing email delivery for package {PackageId}", id);
        
        // Get package details
        var package = await _repository.GetByIdAsync(id);
        if (package == null)
            return NotFound();
        
        if (package.Status != PackageStatus.Generated)
            return BadRequest("Package must be generated before sending");
        
        // Create communication log
        var communicationId = Guid.NewGuid();
        var communicationLog = new CommunicationLog
        {
            CommunicationId = communicationId,
            DemandPackageId = package.PackageId,
            TenantId = package.TenantId,
            Action = CommunicationAction.InitialDemand,
            Channel = CommunicationChannel.Email,
            RecipientsJson = System.Text.Json.JsonSerializer.Serialize(request.Recipients),
            CcRecipientsJson = request.CcRecipients != null 
                ? System.Text.Json.JsonSerializer.Serialize(request.CcRecipients) 
                : null,
            EmailSubject = request.Subject,
            EmailBody = request.Body,
            FromAddress = "noreply@subrogationsaas.com",
            Status = CommunicationStatus.Queued,
            InitiatedBy = User.Identity?.Name ?? "System",
            CreatedAt = DateTime.UtcNow
        };

        await _communicationRepository.CreateAsync(communicationLog);
        
        // Send message to Service Bus queue 'email-delivery'
        var message = new EmailDeliveryMessage
        {
            CommunicationId = communicationId,
            PackageId = package.PackageId,
            TenantId = package.TenantId,
            Recipients = request.Recipients,
            CcRecipients = request.CcRecipients,
            Subject = request.Subject,
            Body = request.Body,
            PdfBlobPath = package.GeneratedPdfPath,
            RequestedAt = DateTime.UtcNow
        };
        
        await _serviceBus.SendMessageAsync("email-delivery", message);
        
        _logger.LogInformation("Email delivery queued for package {PackageId}, communication {CommunicationId}", 
            id, communicationId);
        
        return Accepted(new { message = "Email delivery queued", packageId = id, communicationId });
    }

    /// <summary>
    /// Upload file to package
    /// </summary>
    [HttpPost("{id}/upload")]
    [RequestSizeLimit(52428800)] // 50 MB limit
    public async Task<ActionResult<PackageDocument>> UploadFile(
        Guid id,
        [FromForm] IFormFile file,
        [FromForm] string documentName,
        [FromForm] DocumentType documentType = DocumentType.Other,
        [FromForm] int displayOrder = 0,
        [FromForm] bool isIncluded = true,
        [FromForm] bool isSensitive = false)
    {
        _logger.LogInformation("Uploading file to package {PackageId}: {FileName}", id, file?.FileName);
        
        // Validate package exists
        var package = await _repository.GetByIdAsync(id);
        if (package == null)
            return NotFound($"Package {id} not found");
        
        // Validate file
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");
        
        // Validate file size (50 MB max)
        if (file.Length > 52428800)
            return BadRequest("File size exceeds 50 MB limit");
        
        // Validate file type
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        
        try
        {
            // Upload to blob storage
            string blobPath;
            using (var stream = file.OpenReadStream())
            {
                blobPath = await _blobStorage.UploadDocumentAsync(
                    package.TenantId,
                    package.PackageId,
                    file.FileName,
                    stream,
                    file.ContentType);
            }
            
            // Create document record
            var document = new PackageDocument
            {
                PackageDocumentId = Guid.NewGuid(),
                DemandPackageId = id,
                DocumentName = string.IsNullOrWhiteSpace(documentName) ? file.FileName : documentName,
                Type = documentType,
                Source = DocumentSource.UserUpload,
                BlobStoragePath = blobPath,
                DisplayOrder = displayOrder,
                IsIncluded = isIncluded,
                IsSensitive = isSensitive,
                UploadedAt = DateTime.UtcNow
            };
            
            await _repository.AddDocumentAsync(document);
            
            _logger.LogInformation(
                "File uploaded successfully to package {PackageId}: {DocumentId} - {BlobPath}",
                id, document.PackageDocumentId, blobPath);
            
            return CreatedAtAction(nameof(GetPackage), new { id }, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to package {PackageId}", id);
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    /// <summary>
    /// Get documents for a package
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<IEnumerable<PackageDocument>>> GetDocuments(Guid id)
    {
        _logger.LogInformation("Fetching documents for package {PackageId}", id);
        
        var package = await _repository.GetByIdWithDocumentsAsync(id);
        if (package == null)
            return NotFound($"Package {id} not found");
        
        return Ok(package.Documents.OrderBy(d => d.DisplayOrder));
    }

    /// <summary>
    /// Delete document from package
    /// </summary>
    [HttpDelete("{id}/documents/{documentId}")]
    public async Task<ActionResult> DeleteDocument(Guid id, Guid documentId)
    {
        _logger.LogInformation("Deleting document {DocumentId} from package {PackageId}", documentId, id);
        
        var package = await _repository.GetByIdWithDocumentsAsync(id);
        if (package == null)
            return NotFound($"Package {id} not found");
        
        var document = package.Documents.FirstOrDefault(d => d.PackageDocumentId == documentId);
        if (document == null)
            return NotFound($"Document {documentId} not found");
        
        try
        {
            // Delete from blob storage
            await _blobStorage.DeleteBlobAsync(document.BlobStoragePath, "documents");
            
            // Delete from database
            await _repository.DeleteDocumentAsync(documentId);
            
            _logger.LogInformation("Document {DocumentId} deleted successfully", documentId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while deleting the document");
        }
    }

    /// <summary>
    /// Download document
    /// </summary>
    [HttpGet("{id}/documents/{documentId}/download")]
    public async Task<ActionResult> DownloadDocument(Guid id, Guid documentId)
    {
        _logger.LogInformation("Downloading document {DocumentId} from package {PackageId}", documentId, id);
        
        var package = await _repository.GetByIdWithDocumentsAsync(id);
        if (package == null)
            return NotFound($"Package {id} not found");
        
        var document = package.Documents.FirstOrDefault(d => d.PackageDocumentId == documentId);
        if (document == null)
            return NotFound($"Document {documentId} not found");
        
        try
        {
            var stream = await _blobStorage.DownloadDocumentAsync(document.BlobStoragePath);
            var fileName = Path.GetFileName(document.BlobStoragePath);
            
            // Determine content type
            var contentType = document.BlobStoragePath.EndsWith(".pdf") ? "application/pdf" :
                             document.BlobStoragePath.EndsWith(".jpg") || document.BlobStoragePath.EndsWith(".jpeg") ? "image/jpeg" :
                             document.BlobStoragePath.EndsWith(".png") ? "image/png" :
                             "application/octet-stream";
            
            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while downloading the document");
        }
    }
}

public class SendPackageRequest
{
    public List<string> Recipients { get; set; } = new();
    public List<string>? CcRecipients { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
