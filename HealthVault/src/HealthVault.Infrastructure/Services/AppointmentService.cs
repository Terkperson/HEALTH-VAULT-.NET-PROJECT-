using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Mapping;
using HealthVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HealthVaultDbContext _db;
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes(30);

    public AppointmentService(HealthVaultDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<IReadOnlyList<AppointmentDto>>> ListAsync(
        string actorUserId, IEnumerable<string> roles, Guid? patientId, byte? status, DateOnly? date, CancellationToken ct = default)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .AsQueryable();

        if (roleSet.Contains(UserRoles.Patient) && !roleSet.Contains(UserRoles.Staff) && !roleSet.Contains(UserRoles.Administrator))
        {
            query = query.Where(a => a.Patient.UserId == actorUserId);
        }
        else if (patientId.HasValue)
        {
            query = query.Where(a => a.PatientId == patientId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == (AppointmentStatus)status.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(a => a.AppointmentDate == date.Value);
        }

        var items = await query
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<AppointmentDto>>.Ok(items.Select(a => a.ToDto()).ToList());
    }

    public async Task<ApiResponse<AppointmentDto>> GetByIdAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var appointment = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return ApiResponse<AppointmentDto>.Fail("Appointment not found.");
        if (!CanView(appointment, actorUserId, roles))
            return ApiResponse<AppointmentDto>.Fail("You are not allowed to view this appointment.");

        return ApiResponse<AppointmentDto>.Ok(appointment.ToDto());
    }

    public async Task<ApiResponse<AppointmentDto>> CreateAsync(CreateAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var end = request.EndTime ?? request.StartTime.Add(DefaultDuration);

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId && p.IsActive, ct);
        if (patient is null) return ApiResponse<AppointmentDto>.Fail("Patient not found.");

        if (roleSet.Contains(UserRoles.Patient) && !roleSet.Contains(UserRoles.Staff) && !roleSet.Contains(UserRoles.Administrator)
            && patient.UserId != actorUserId)
        {
            return ApiResponse<AppointmentDto>.Fail("Patients can only book appointments for themselves.");
        }

        var staff = await _db.StaffMembers.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.IsActive, ct);
        if (staff is null) return ApiResponse<AppointmentDto>.Fail("The selected clinician was not found.");

        var slotError = ValidateSlot(request.AppointmentDate, request.StartTime, end);
        if (slotError is not null) return ApiResponse<AppointmentDto>.Fail(slotError);

        if (await IsDoubleBookedAsync(request.StaffId, request.AppointmentDate, request.StartTime, end, null, ct))
        {
            return ApiResponse<AppointmentDto>.Fail("That clinician already has an appointment in this time slot.");
        }

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            StaffId = staff.Id,
            AppointmentDate = request.AppointmentDate,
            StartTime = request.StartTime,
            EndTime = end,
            Status = AppointmentStatus.Pending,
            Reason = request.Reason.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedByUserId = actorUserId
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync(ct);

        appointment.Patient = patient;
        appointment.Staff = staff;
        return ApiResponse<AppointmentDto>.Ok(appointment.ToDto(), "Appointment booked.");
    }

    public async Task<ApiResponse<AppointmentDto>> UpdateAsync(Guid id, UpdateAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return ApiResponse<AppointmentDto>.Fail("Appointment not found.");
        if (!CanManage(appointment, actorUserId, roles))
            return ApiResponse<AppointmentDto>.Fail("You are not allowed to update this appointment.");
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return ApiResponse<AppointmentDto>.Fail("Completed or cancelled appointments cannot be rescheduled.");

        var end = request.EndTime ?? request.StartTime.Add(DefaultDuration);
        var slotError = ValidateSlot(request.AppointmentDate, request.StartTime, end);
        if (slotError is not null) return ApiResponse<AppointmentDto>.Fail(slotError);

        var staff = await _db.StaffMembers.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.IsActive, ct);
        if (staff is null) return ApiResponse<AppointmentDto>.Fail("The selected clinician was not found.");

        if (await IsDoubleBookedAsync(request.StaffId, request.AppointmentDate, request.StartTime, end, appointment.Id, ct))
        {
            return ApiResponse<AppointmentDto>.Fail("That clinician already has an appointment in this time slot.");
        }

        appointment.StaffId = staff.Id;
        appointment.Staff = staff;
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.StartTime = request.StartTime;
        appointment.EndTime = end;
        appointment.Reason = request.Reason.Trim();
        appointment.Notes = request.Notes?.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AppointmentDto>.Ok(appointment.ToDto(), "Appointment updated.");
    }

    public async Task<ApiResponse<AppointmentDto>> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(AppointmentStatus), request.Status))
            return ApiResponse<AppointmentDto>.Fail("Invalid appointment status.");

        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return ApiResponse<AppointmentDto>.Fail("Appointment not found.");

        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var isStaff = roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator);
        if (!isStaff)
            return ApiResponse<AppointmentDto>.Fail("Only clinic staff can change appointment status.");

        var next = (AppointmentStatus)request.Status;
        if (next == AppointmentStatus.Cancelled)
        {
            return await CancelAsync(id, new CancelAppointmentRequest
            {
                CancellationReason = request.CancellationReason ?? "Cancelled by staff"
            }, actorUserId, roles, ct);
        }

        appointment.Status = next;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ApiResponse<AppointmentDto>.Ok(appointment.ToDto(), "Status updated.");
    }

    public async Task<ApiResponse<AppointmentDto>> CancelAsync(Guid id, CancelAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (appointment is null) return ApiResponse<AppointmentDto>.Fail("Appointment not found.");
        if (!CanManage(appointment, actorUserId, roles))
            return ApiResponse<AppointmentDto>.Fail("You are not allowed to cancel this appointment.");
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return ApiResponse<AppointmentDto>.Fail("This appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = request.CancellationReason.Trim();
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelledByUserId = actorUserId;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ApiResponse<AppointmentDto>.Ok(appointment.ToDto(), "Appointment cancelled.");
    }

    public async Task<ApiResponse<IReadOnlyList<StaffOptionDto>>> ListStaffAsync(CancellationToken ct = default)
    {
        var staff = await _db.StaffMembers.AsNoTracking()
            .Include(s => s.Department)
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName)
            .Select(s => new StaffOptionDto
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
                Specialization = s.Specialization,
                Department = s.Department != null ? s.Department.Name : null
            })
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<StaffOptionDto>>.Ok(staff);
    }

    private async Task<bool> IsDoubleBookedAsync(Guid staffId, DateOnly date, TimeOnly start, TimeOnly end, Guid? exceptId, CancellationToken ct)
    {
        return await _db.Appointments.AnyAsync(a =>
            a.StaffId == staffId &&
            a.AppointmentDate == date &&
            a.Status != AppointmentStatus.Cancelled &&
            (!exceptId.HasValue || a.Id != exceptId.Value) &&
            a.StartTime < end &&
            start < a.EndTime, ct);
    }

    private static string? ValidateSlot(DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (end <= start) return "End time must be after start time.";
        if (date < DateOnly.FromDateTime(DateTime.UtcNow)) return "Appointments cannot be booked in the past.";
        if (start < new TimeOnly(8, 0) || end > new TimeOnly(18, 0))
            return "Clinic hours are 08:00 to 18:00.";
        return null;
    }

    private static bool CanView(Appointment appointment, string actorUserId, IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator)) return true;
        return appointment.Patient?.UserId == actorUserId;
    }

    private static bool CanManage(Appointment appointment, string actorUserId, IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator)) return true;
        return appointment.Patient?.UserId == actorUserId && appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed;
    }
}
