using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Auth;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Identity;
using HealthVault.Infrastructure.Persistence;
using HealthVault.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly HealthVaultDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthService(UserManager<ApplicationUser> users, HealthVaultDbContext db, JwtTokenService jwt)
    {
        _users = users;
        _db = db;
        _jwt = jwt;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.FindByEmailAsync(email) is not null)
        {
            return ApiResponse<AuthResponse>.Fail("An account with this email already exists.");
        }

        if (!Enum.IsDefined(typeof(Gender), request.Gender))
        {
            return ApiResponse<AuthResponse>.Fail("Please select a valid gender.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.Phone.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _users.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            return ApiResponse<AuthResponse>.Fail("Registration failed.", created.Errors.Select(e => e.Description));
        }

        await _users.AddToRoleAsync(user, UserRoles.Patient);

        var patient = new Patient
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = (Gender)request.Gender,
            Email = email,
            Phone = request.Phone.Trim(),
            Country = "Ghana",
            IsActive = true,
            CreatedByUserId = user.Id
        };
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AuthResponse>.Ok(await BuildAuthAsync(user, patient.Id, null), "Account created.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        if (!await _users.CheckPasswordAsync(user, request.Password))
        {
            return ApiResponse<AuthResponse>.Fail("Invalid email or password.");
        }

        var patientId = await _db.Patients.AsNoTracking()
            .Where(p => p.UserId == user.Id)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);

        var staffId = await _db.StaffMembers.AsNoTracking()
            .Where(s => s.UserId == user.Id)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);

        return ApiResponse<AuthResponse>.Ok(await BuildAuthAsync(user, patientId, staffId), "Signed in.");
    }

    private async Task<AuthResponse> BuildAuthAsync(ApplicationUser user, Guid? patientId, Guid? staffId)
    {
        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = _jwt.CreateToken(user, roles);
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expires,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? UserRoles.Patient,
            PatientId = patientId,
            StaffId = staffId
        };
    }
}
