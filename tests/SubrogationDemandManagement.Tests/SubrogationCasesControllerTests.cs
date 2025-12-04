using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SubrogationDemandManagement.API.Controllers;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data.Repositories;

using SubrogationDemandManagement.Services.Auth;

namespace SubrogationDemandManagement.Tests;

public class SubrogationCasesControllerTests
{
    private readonly Mock<ISubrogationCaseRepository> _mockRepository;
    private readonly Mock<ILogger<SubrogationCasesController>> _mockLogger;
    private readonly Mock<ICurrentTenantService> _mockTenantService;
    private readonly SubrogationCasesController _controller;

    public SubrogationCasesControllerTests()
    {
        _mockRepository = new Mock<ISubrogationCaseRepository>();
        _mockLogger = new Mock<ILogger<SubrogationCasesController>>();
        _mockTenantService = new Mock<ICurrentTenantService>();
        _controller = new SubrogationCasesController(_mockLogger.Object, _mockRepository.Object, _mockTenantService.Object);
    }

    [Fact]
    public async Task GetCase_ShouldReturnOk_WhenCaseExists()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        var subrogationCase = new SubrogationCase { CaseId = caseId, ClaimId = "TEST-001" };
        _mockRepository.Setup(r => r.GetByIdAsync(caseId)).ReturnsAsync(subrogationCase);

        // Act
        var result = await _controller.GetCase(caseId);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCase = Assert.IsType<SubrogationCase>(actionResult.Value);
        Assert.Equal(caseId, returnedCase.CaseId);
    }

    [Fact]
    public async Task GetCase_ShouldReturnNotFound_WhenCaseDoesNotExist()
    {
        // Arrange
        var caseId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(caseId)).ReturnsAsync((SubrogationCase?)null);

        // Act
        var result = await _controller.GetCase(caseId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateCase_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var subrogationCase = new SubrogationCase { ClaimId = "NEW-CLAIM" };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<SubrogationCase>()))
            .ReturnsAsync((SubrogationCase c) => 
            {
                c.CaseId = Guid.NewGuid(); // Simulate DB ID generation (though controller does it too)
                return c;
            });

        // Act
        var result = await _controller.CreateCase(subrogationCase);

        // Assert
        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(SubrogationCasesController.GetCase), actionResult.ActionName);
        var createdCase = Assert.IsType<SubrogationCase>(actionResult.Value);
        Assert.Equal("NEW-CLAIM", createdCase.ClaimId);
    }

    [Fact]
    public async Task GetCasesByStatus_ShouldReturnList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var status = CaseStatus.Draft;
        var cases = new List<SubrogationCase>
        {
            new SubrogationCase { CaseId = Guid.NewGuid(), Status = status },
            new SubrogationCase { CaseId = Guid.NewGuid(), Status = status }
        };

        _mockRepository.Setup(r => r.GetByTenantAndStatus(tenantId, status)).ReturnsAsync(cases);

        // Act
        var result = await _controller.GetCasesByStatus(tenantId, status);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCases = Assert.IsType<List<SubrogationCase>>(actionResult.Value);
        Assert.Equal(2, returnedCases.Count);
    }
}
