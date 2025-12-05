using Microsoft.AspNetCore.Mvc;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Messaging;
using SubrogationDemandManagement.Services.Messaging.Messages;

namespace SubrogationDemandManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemandPackagesController : ControllerBase
{
    private readonly ILogger<DemandPackagesController> _logger;
    private readonly DemandPackageRepository _repository;
    private readonly CommunicationLogRepository _communicationRepository;
    private readonly ServiceBusService _serviceBus;

    public DemandPackagesController(
        ILogger<DemandPackagesController> logger,
        DemandPackageRepository repository,
        CommunicationLogRepository communicationRepository,
        ServiceBusService serviceBus)
    {
        _logger = logger;
        _repository = repository;
        _communicationRepository = communicationRepository;
        _serviceBus = serviceBus;
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
    /// Add document to package
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<ActionResult> AddDocument(Guid id, [FromBody] PackageDocument document)
    {
        _logger.LogInformation("Adding document to package {PackageId}", id);
        
        document.PackageDocumentId = Guid.NewGuid();
        document.DemandPackageId = id;
        document.UploadedAt = DateTime.UtcNow;
        
        await _repository.AddDocumentAsync(document);
        
        return CreatedAtAction(nameof(GetPackage), new { id }, document);
    }
}

public class SendPackageRequest
{
    public List<string> Recipients { get; set; } = new();
    public List<string>? CcRecipients { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
