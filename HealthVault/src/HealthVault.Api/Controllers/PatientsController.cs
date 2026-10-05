using HealthVault.Application.DTOs.Patients;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController(IPatientService patients) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Staff},{UserRoles.Administrator}")]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await patients.SearchAsync(new PatientSearchRequest { Query = query, Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = UserRoles.Patient)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await patients.GetMineAsync(UserId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await patients.GetByIdAsync(id, ct);
        if (!result.Success) return NotFound(result);

        if (User.IsInRole(UserRoles.Patient) && result.Data?.UserId != UserId)
            return Forbid();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Staff},{UserRoles.Administrator}")]
    public async Task<IActionResult> Create([FromBody] UpsertPatientRequest request, CancellationToken ct)
    {
        var result = await patients.CreateAsync(request, UserId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPatientRequest request, CancellationToken ct)
    {
        var result = await patients.UpdateAsync(id, request, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
