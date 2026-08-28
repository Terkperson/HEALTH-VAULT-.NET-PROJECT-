using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Dashboard;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Identity;
using HealthVault.Infrastructure.Mapping;
using HealthVault.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly HealthVaultDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DashboardService(HealthVaultDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<ApiResponse<PatientDashboardDto>> GetPatientAsync(string userId, CancellationToken ct = default)
    {
        var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (patient is null) return ApiResponse<PatientDashboardDto>.Fail("Patient profile not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcoming = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .Where(a => a.PatientId == patient.Id
                        && a.AppointmentDate >= today
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(5)
            .ToListAsync(ct);

        var records = await _db.MedicalRecords.AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Category)
            .Where(r => r.PatientId == patient.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(ct);

        return ApiResponse<PatientDashboardDto>.Ok(new PatientDashboardDto
        {
            FullName = patient.FullName,
            UpcomingAppointmentCount = upcoming.Count,
            RecordCount = await _db.MedicalRecords.CountAsync(r => r.PatientId == patient.Id, ct),
            UpcomingAppointments = upcoming.Select(a => a.ToDto()).ToList(),
            RecentRecords = records.Select(r => r.ToDto()).ToList()
        });
    }

    public async Task<ApiResponse<StaffDashboardDto>> GetStaffAsync(string userId, CancellationToken ct = default)
    {
        var staff = await _db.StaffMembers.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var todayAppointments = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .Where(a => a.AppointmentDate == today && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);

        var pending = await _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Staff)
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(8)
            .ToListAsync(ct);

        var recentPatients = await _db.Patients.CountAsync(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-14), ct);

        return ApiResponse<StaffDashboardDto>.Ok(new StaffDashboardDto
        {
            FullName = staff?.FullName ?? "Clinic Staff",
            TodayAppointmentCount = todayAppointments.Count,
            PendingAppointmentCount = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending, ct),
            RecentPatientCount = recentPatients,
            TodayAppointments = todayAppointments.Select(a => a.ToDto()).ToList(),
            PendingAppointments = pending.Select(a => a.ToDto()).ToList()
        });
    }

    public async Task<ApiResponse<AdminDashboardDto>> GetAdminAsync(CancellationToken ct = default)
    {
        var statuses = await _db.Appointments
            .GroupBy(a => a.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int Count(AppointmentStatus status) => statuses.FirstOrDefault(s => s.Key == status)?.Count ?? 0;

        return ApiResponse<AdminDashboardDto>.Ok(new AdminDashboardDto
        {
            UserCount = await _users.Users.CountAsync(ct),
            PatientCount = await _db.Patients.CountAsync(p => p.IsActive, ct),
            StaffCount = await _db.StaffMembers.CountAsync(s => s.IsActive, ct),
            AppointmentCount = await _db.Appointments.CountAsync(ct),
            PendingAppointments = Count(AppointmentStatus.Pending),
            ConfirmedAppointments = Count(AppointmentStatus.Confirmed),
            CompletedAppointments = Count(AppointmentStatus.Completed),
            CancelledAppointments = Count(AppointmentStatus.Cancelled),
            MedicalRecordCount = await _db.MedicalRecords.CountAsync(ct)
        });
    }
}
