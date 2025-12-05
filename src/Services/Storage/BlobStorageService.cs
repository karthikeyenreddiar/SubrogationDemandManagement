using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SubrogationDemandManagement.Services.Storage;

/// <summary>
/// Service for managing documents and PDFs in Azure Blob Storage
/// </summary>
public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _documentsContainer = "documents";
    private readonly string _packagesContainer = "packages";
    private readonly string _templatesContainer = "templates";

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["BlobStorage:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Blob Storage connection string not configured. Using development storage.");
            connectionString = "UseDevelopmentStorage=true";
        }
        
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    /// <summary>
    /// Upload document to blob storage
    /// </summary>
    public virtual async Task<string> UploadDocumentAsync(
        Guid tenantId, 
        Guid packageId, 
        string fileName, 
        Stream content,
        string contentType = "application/pdf")
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_documentsContainer);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobPath = $"{tenantId}/{packageId}/{fileName}";
        var blobClient = containerClient.GetBlobClient(blobPath);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, uploadOptions);
        
        _logger.LogInformation("Document uploaded to blob storage: {BlobPath}", blobPath);
        
        return blobPath;
    }

    /// <summary>
    /// Upload generated PDF package
    /// </summary>
    public virtual async Task<string> UploadPackageAsync(
        Guid tenantId, 
        Guid packageId, 
        Stream pdfContent)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_packagesContainer);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobPath = $"{tenantId}/{packageId}/package.pdf";
        var blobClient = containerClient.GetBlobClient(blobPath);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
        };

        await blobClient.UploadAsync(pdfContent, uploadOptions);
        
        _logger.LogInformation("Package PDF uploaded to blob storage: {BlobPath}", blobPath);
        
        return blobPath;
    }

    /// <summary>
    /// Download blob as stream
    /// </summary>
    public virtual async Task<Stream> DownloadBlobAsync(string blobPath, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    /// <summary>
    /// Download document
    /// </summary>
    public virtual async Task<Stream> DownloadDocumentAsync(string blobPath)
    {
        return await DownloadBlobAsync(blobPath, _documentsContainer);
    }

    /// <summary>
    /// Download package PDF
    /// </summary>
    public virtual async Task<Stream> DownloadPackageAsync(string blobPath)
    {
        return await DownloadBlobAsync(blobPath, _packagesContainer);
    }

    /// <summary>
    /// Download template
    /// </summary>
    public virtual async Task<Stream> DownloadTemplateAsync(string blobPath)
    {
        return await DownloadBlobAsync(blobPath, _templatesContainer);
    }

    /// <summary>
    /// Delete blob
    /// </summary>
    public virtual async Task DeleteBlobAsync(string blobPath, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.DeleteIfExistsAsync();
        
        _logger.LogInformation("Blob deleted: {BlobPath}", blobPath);
    }

    /// <summary>
    /// Get blob properties (size, content type, etc.)
    /// </summary>
    public virtual async Task<BlobProperties> GetBlobPropertiesAsync(string blobPath, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var properties = await blobClient.GetPropertiesAsync();
        return properties.Value;
    }
}
