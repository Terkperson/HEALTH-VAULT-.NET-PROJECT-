using System.Security.Claims;
using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? patientId, [FromQuery] byte? status, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var result = await _appointments.ListAsync(UserId, Roles, patientId, status, date, ct);
        return Ok(result);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> Staff(CancellationToken ct)
    {
        var result = await _appointments.ListStaffAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _appointments.GetByIdAsync(id, UserId, Roles, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointments.CreateAsync(request, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointments.UpdateAsync(id, request, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = $"{UserRoles.Staff},{UserRoles.Administrator}")]
    public async Task<IActionResult> Status(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var result = await _appointments.UpdateStatusAsync(id, request, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointments.CancelAsync(id, request, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private IEnumerable<string> Roles => User.FindAll(ClaimTypes.Role).Select(c => c.Value);
}
