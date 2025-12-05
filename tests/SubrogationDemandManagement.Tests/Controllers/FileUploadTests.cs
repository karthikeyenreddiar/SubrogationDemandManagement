using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.API.Controllers;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Messaging;
using SubrogationDemandManagement.Services.Storage;
using System.Text;

namespace SubrogationDemandManagement.Tests.Controllers;

public class FileUploadTests
{
    private readonly Mock<ILogger<DemandPackagesController>> _mockLogger;
    private readonly Mock<DemandPackageRepository> _mockRepository;
    private readonly Mock<CommunicationLogRepository> _mockCommunicationRepository;
    private readonly Mock<ServiceBusService> _mockServiceBus;
    private readonly Mock<BlobStorageService> _mockBlobStorage;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly DemandPackagesController _controller;

    public FileUploadTests()
    {
        _mockLogger = new Mock<ILogger<DemandPackagesController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Setup configuration to return null/empty for connection strings to avoid actual client creation
        _mockConfiguration.Setup(c => c["ServiceBus:ConnectionString"]).Returns(string.Empty);
        _mockConfiguration.Setup(c => c["BlobStorage:ConnectionString"]).Returns(string.Empty);

        _mockRepository = new Mock<DemandPackageRepository>(null);
        _mockCommunicationRepository = new Mock<CommunicationLogRepository>(null);
        
        // Pass configuration to service mocks
        _mockServiceBus = new Mock<ServiceBusService>(_mockConfiguration.Object, new Mock<ILogger<ServiceBusService>>().Object);
        _mockBlobStorage = new Mock<BlobStorageService>(_mockConfiguration.Object, new Mock<ILogger<BlobStorageService>>().Object);

        _controller = new DemandPackagesController(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockCommunicationRepository.Object,
            _mockServiceBus.Object,
            _mockBlobStorage.Object);
    }

    [Fact]
    public async Task UploadFile_WithValidFile_ReturnsCreatedResult()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new DemandPackage
        {
            PackageId = packageId,
            TenantId = Guid.NewGuid(),
            SubrogationCaseId = Guid.NewGuid(),
            Status = PackageStatus.Draft
        };

        _mockRepository.Setup(r => r.GetByIdAsync(packageId))
            .ReturnsAsync(package);

        _mockBlobStorage.Setup(b => b.UploadDocumentAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>()))
            .ReturnsAsync("tenant/package/test.pdf");

        _mockRepository.Setup(r => r.AddDocumentAsync(It.IsAny<PackageDocument>()))
            .Returns(Task.CompletedTask);

        var file = CreateMockFormFile("test.pdf", "application/pdf", 1024);

        // Act
        var result = await _controller.UploadFile(
            packageId,
            file,
            "Test Document",
            DocumentType.Other);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var document = Assert.IsType<PackageDocument>(createdResult.Value);
        Assert.Equal("Test Document", document.DocumentName);
        Assert.Equal(DocumentType.Other, document.Type);
        Assert.Equal(DocumentSource.UserUpload, document.Source);
    }

    [Fact]
    public async Task UploadFile_WithNonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(packageId))
            .ReturnsAsync((DemandPackage?)null);

        var file = CreateMockFormFile("test.pdf", "application/pdf", 1024);

        // Act
        var result = await _controller.UploadFile(
            packageId,
            file,
            "Test Document");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UploadFile_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new DemandPackage { PackageId = packageId };

        _mockRepository.Setup(r => r.GetByIdAsync(packageId))
            .ReturnsAsync(package);

        // Act
        var result = await _controller.UploadFile(
            packageId,
            null!,
            "Test Document");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UploadFile_WithTooLargeFile_ReturnsBadRequest()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new DemandPackage { PackageId = packageId };

        _mockRepository.Setup(r => r.GetByIdAsync(packageId))
            .ReturnsAsync(package);

        var file = CreateMockFormFile("large.pdf", "application/pdf", 60 * 1024 * 1024); // 60 MB

        // Act
        var result = await _controller.UploadFile(
            packageId,
            file,
            "Large Document");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("exceeds 50 MB", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task UploadFile_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new DemandPackage { PackageId = packageId };

        _mockRepository.Setup(r => r.GetByIdAsync(packageId))
            .ReturnsAsync(package);

        var file = CreateMockFormFile("test.exe", "application/x-msdownload", 1024);

        // Act
        var result = await _controller.UploadFile(
            packageId,
            file,
            "Invalid Document");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("not allowed", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task GetDocuments_WithValidPackage_ReturnsDocuments()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new DemandPackage
        {
            PackageId = packageId,
            Documents = new List<PackageDocument>
            {
                new PackageDocument
                {
                    PackageDocumentId = Guid.NewGuid(),
                    DocumentName = "Doc1",
                    DisplayOrder = 1
                },
                new PackageDocument
                {
                    PackageDocumentId = Guid.NewGuid(),
                    DocumentName = "Doc2",
                    DisplayOrder = 2
                }
            }
        };

        _mockRepository.Setup(r => r.GetByIdWithDocumentsAsync(packageId))
            .ReturnsAsync(package);

        // Act
        var result = await _controller.GetDocuments(packageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var documents = Assert.IsAssignableFrom<IEnumerable<PackageDocument>>(okResult.Value);
        Assert.Equal(2, documents.Count());
    }

    [Fact]
    public async Task DeleteDocument_WithValidDocument_ReturnsNoContent()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var package = new DemandPackage
        {
            PackageId = packageId,
            Documents = new List<PackageDocument>
            {
                new PackageDocument
                {
                    PackageDocumentId = documentId,
                    BlobStoragePath = "tenant/package/test.pdf"
                }
            }
        };

        _mockRepository.Setup(r => r.GetByIdWithDocumentsAsync(packageId))
            .ReturnsAsync(package);

        _mockBlobStorage.Setup(b => b.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.DeleteDocumentAsync(documentId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteDocument(packageId, documentId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockBlobStorage.Verify(b => b.DeleteBlobAsync(It.IsAny<string>(), "documents"), Times.Once);
        _mockRepository.Verify(r => r.DeleteDocumentAsync(documentId), Times.Once);
    }

    private IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var content = new byte[length];
        var stream = new MemoryStream(content);
        var file = new Mock<IFormFile>();
        
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.Length).Returns(length);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));

        return file.Object;
    }
}
