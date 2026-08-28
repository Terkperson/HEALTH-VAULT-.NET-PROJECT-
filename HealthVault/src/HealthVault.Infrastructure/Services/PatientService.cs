using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Patients;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Mapping;
using HealthVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly HealthVaultDbContext _db;

    public PatientService(HealthVaultDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<PagedResult<PatientDto>>> SearchAsync(PatientSearchRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var query = _db.Patients.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var term = request.Query.Trim();
            query = query.Where(p =>
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.Email != null && p.Email.Contains(term)) ||
                p.Phone.Contains(term) ||
                (p.GhanaCardNumber != null && p.GhanaCardNumber.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return ApiResponse<PagedResult<PatientDto>>.Ok(new PagedResult<PatientDto>
        {
            Items = items.Select(p => p.ToDto()).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PatientDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return patient is null
            ? ApiResponse<PatientDto>.Fail("Patient not found.")
            : ApiResponse<PatientDto>.Ok(patient.ToDto());
    }

    public async Task<ApiResponse<PatientDto>> GetMineAsync(string userId, CancellationToken ct = default)
    {
        var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return patient is null
            ? ApiResponse<PatientDto>.Fail("No patient profile is linked to this account.")
            : ApiResponse<PatientDto>.Ok(patient.ToDto());
    }

    public async Task<ApiResponse<PatientDto>> CreateAsync(UpsertPatientRequest request, string createdByUserId, CancellationToken ct = default)
    {
        var validation = Validate(request);
        if (validation is not null) return validation;

        var patient = Apply(new Patient
        {
            CreatedByUserId = createdByUserId,
            IsActive = true
        }, request);

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);
        return ApiResponse<PatientDto>.Ok(patient.ToDto(), "Patient created.");
    }

    public async Task<ApiResponse<PatientDto>> UpdateAsync(Guid id, UpsertPatientRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var validation = Validate(request);
        if (validation is not null) return validation;

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (patient is null) return ApiResponse<PatientDto>.Fail("Patient not found.");

        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var isOwner = patient.UserId == actorUserId;
        var isStaff = roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator);
        if (!isOwner && !isStaff)
        {
            return ApiResponse<PatientDto>.Fail("You are not allowed to update this profile.");
        }

        Apply(patient, request);
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ApiResponse<PatientDto>.Ok(patient.ToDto(), "Patient updated.");
    }

    private static ApiResponse<PatientDto>? Validate(UpsertPatientRequest request)
    {
        if (!Enum.IsDefined(typeof(Gender), request.Gender))
            return ApiResponse<PatientDto>.Fail("Please select a valid gender.");
        if (request.BloodGroup.HasValue && !Enum.IsDefined(typeof(BloodGroup), request.BloodGroup.Value))
            return ApiResponse<PatientDto>.Fail("Please select a valid blood group.");
        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            return ApiResponse<PatientDto>.Fail("Date of birth cannot be in the future.");
        return null;
    }

    private static Patient Apply(Patient patient, UpsertPatientRequest request)
    {
        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = (Gender)request.Gender;
        patient.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        patient.Phone = request.Phone.Trim();
        patient.GhanaCardNumber = string.IsNullOrWhiteSpace(request.GhanaCardNumber) ? null : request.GhanaCardNumber.Trim();
        patient.BloodGroup = request.BloodGroup.HasValue ? (BloodGroup)request.BloodGroup.Value : null;
        patient.AddressLine1 = request.AddressLine1?.Trim();
        patient.AddressLine2 = request.AddressLine2?.Trim();
        patient.City = request.City?.Trim();
        patient.Region = request.Region?.Trim();
        patient.PostalCode = request.PostalCode?.Trim();
        patient.Country = string.IsNullOrWhiteSpace(request.Country) ? "Ghana" : request.Country.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        patient.EmergencyContactRelation = request.EmergencyContactRelation?.Trim();
        patient.Allergies = request.Allergies?.Trim();
        patient.Notes = request.Notes?.Trim();
        return patient;
    }
}
