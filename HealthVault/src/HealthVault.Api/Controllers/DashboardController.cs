using System.Security.Claims;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboards;

    public DashboardController(IDashboardService dashboards)
    {
        _dashboards = dashboards;
    }

    [HttpGet("patient")]
    [Authorize(Roles = UserRoles.Patient)]
    public async Task<IActionResult> Patient(CancellationToken ct)
    {
        var result = await _dashboards.GetPatientAsync(UserId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("staff")]
    [Authorize(Roles = $"{UserRoles.Staff},{UserRoles.Administrator}")]
    public async Task<IActionResult> Staff(CancellationToken ct)
    {
        var result = await _dashboards.GetStaffAsync(UserId, ct);
        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = UserRoles.Administrator)]
    public async Task<IActionResult> Admin(CancellationToken ct)
    {
        var result = await _dashboards.GetAdminAsync(ct);
        return Ok(result);
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
