using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HealthVault.Api.Controllers;

/// <summary>
/// Base controller that exposes the authenticated caller's identifier and roles,
/// read from the current <see cref="ControllerBase.User"/> principal.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Gets the authenticated user's identifier, or an empty string when absent.</summary>
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    /// <summary>Gets all role claims assigned to the authenticated user.</summary>
    protected IEnumerable<string> Roles => User.FindAll(ClaimTypes.Role).Select(static c => c.Value);
}
