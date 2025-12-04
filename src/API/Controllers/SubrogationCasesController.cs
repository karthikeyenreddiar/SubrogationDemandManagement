using Microsoft.AspNetCore.Mvc;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Data.Repositories;

namespace SubrogationDemandManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubrogationCasesController : ControllerBase
{
    private readonly ILogger<SubrogationCasesController> _logger;
    private readonly ISubrogationCaseRepository _repository;

    public SubrogationCasesController(
        ILogger<SubrogationCasesController> logger,
        ISubrogationCaseRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Get all subrogation cases for a tenant
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubrogationCase>>> GetCases(
        [FromQuery] Guid tenantId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        _logger.LogInformation("Fetching cases for tenant {TenantId}, skip={Skip}, take={Take}", 
            tenantId, skip, take);
        
        var cases = await _repository.GetByTenantAsync(tenantId, skip, take);
        return Ok(cases);
    }

    /// <summary>
    /// Get cases by tenant and status
    /// </summary>
    [HttpGet("by-status")]
    public async Task<ActionResult<IEnumerable<SubrogationCase>>> GetCasesByStatus(
        [FromQuery] Guid tenantId,
        [FromQuery] CaseStatus status)
    {
        _logger.LogInformation("Fetching cases for tenant {TenantId} with status {Status}", 
            tenantId, status);
        
        var cases = await _repository.GetByTenantAndStatus(tenantId, status);
        return Ok(cases);
    }

    /// <summary>
    /// Get a specific subrogation case by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SubrogationCase>> GetCase(Guid id)
    {
        _logger.LogInformation("Fetching case {CaseId}", id);
        
        var subrogationCase = await _repository.GetByIdAsync(id);
        
        if (subrogationCase == null)
            return NotFound();
            
        return Ok(subrogationCase);
    }

    /// <summary>
    /// Create a new subrogation case
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SubrogationCase>> CreateCase([FromBody] SubrogationCase subrogationCase)
    {
        _logger.LogInformation("Creating new case for claim {ClaimId}", subrogationCase.ClaimId);
        
        subrogationCase.CaseId = Guid.NewGuid();
        subrogationCase.CreatedAt = DateTime.UtcNow;
        subrogationCase.Status = CaseStatus.Draft;
        
        var created = await _repository.CreateAsync(subrogationCase);
        
        return CreatedAtAction(nameof(GetCase), new { id = created.CaseId }, created);
    }

    /// <summary>
    /// Update an existing subrogation case
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCase(Guid id, [FromBody] SubrogationCase subrogationCase)
    {
        _logger.LogInformation("Updating case {CaseId}", id);
        
        if (id != subrogationCase.CaseId)
            return BadRequest("Case ID mismatch");
        
        subrogationCase.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(subrogationCase);
        
        return NoContent();
    }

    /// <summary>
    /// Get count of cases by status
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(
        [FromQuery] Guid tenantId,
        [FromQuery] CaseStatus status)
    {
        var count = await _repository.GetCountByTenantAndStatusAsync(tenantId, status);
        return Ok(count);
    }
}
