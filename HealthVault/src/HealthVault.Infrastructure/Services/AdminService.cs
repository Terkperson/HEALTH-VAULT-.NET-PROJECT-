using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Dashboard;
using HealthVault.Application.Interfaces;
using HealthVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _users;

    public AdminService(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public async Task<ApiResponse<IReadOnlyList<AdminUserDto>>> ListUsersAsync(CancellationToken ct = default)
    {
        var users = await _users.Users.AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        var list = new List<AdminUserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _users.GetRolesAsync(user);
            list.Add(new AdminUserDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Phone = user.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Unassigned",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return ApiResponse<IReadOnlyList<AdminUserDto>>.Ok(list);
    }
}
