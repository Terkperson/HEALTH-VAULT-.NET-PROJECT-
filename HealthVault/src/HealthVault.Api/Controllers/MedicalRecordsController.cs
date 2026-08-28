using System.Security.Claims;
using HealthVault.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _records;

    public MedicalRecordsController(IMedicalRecordService records)
    {
        _records = records;
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> List(Guid patientId, CancellationToken ct)
    {
        var result = await _records.ListForPatientAsync(patientId, UserId, Roles, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _records.GetByIdAsync(id, UserId, Roles, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("patient/{patientId:guid}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid patientId, [FromForm] string title, [FromForm] string? description, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "Please choose a file to upload." });

        await using var stream = file.OpenReadStream();
        var result = await _records.UploadAsync(
            patientId,
            title,
            description,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            UserId,
            Roles,
            ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var result = await _records.DownloadAsync(id, UserId, Roles, ct);
        if (!result.Success || result.Data is null) return NotFound(result);
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    private IEnumerable<string> Roles => User.FindAll(ClaimTypes.Role).Select(c => c.Value);
}
