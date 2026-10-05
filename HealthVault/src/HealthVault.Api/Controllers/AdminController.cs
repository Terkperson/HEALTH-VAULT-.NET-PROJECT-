using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Administrator)]
public class AdminController(IAdminService admin) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct)
    {
        var result = await admin.ListUsersAsync(ct);
        return Ok(result);
    }
}
