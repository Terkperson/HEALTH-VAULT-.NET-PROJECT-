using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService dashboards) : ApiControllerBase
{
    [HttpGet("patient")]
    [Authorize(Roles = UserRoles.Patient)]
    public async Task<IActionResult> Patient(CancellationToken ct)
    {
        var result = await dashboards.GetPatientAsync(UserId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("staff")]
    [Authorize(Roles = $"{UserRoles.Staff},{UserRoles.Administrator}")]
    public async Task<IActionResult> Staff(CancellationToken ct)
    {
        var result = await dashboards.GetStaffAsync(UserId, ct);
        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<IActionResult> Admin(CancellationToken ct)
    {
        var result = await dashboards.GetAdminAsync(ct);
        return Ok(result);
    }
}
