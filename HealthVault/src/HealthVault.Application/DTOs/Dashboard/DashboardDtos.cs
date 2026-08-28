using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.DTOs.Records;

namespace HealthVault.Application.DTOs.Dashboard;

public class PatientDashboardDto
{
    public string FullName { get; set; } = string.Empty;
    public int UpcomingAppointmentCount { get; set; }
    public int RecordCount { get; set; }
    public IReadOnlyList<AppointmentDto> UpcomingAppointments { get; set; } = [];
    public IReadOnlyList<MedicalRecordDto> RecentRecords { get; set; } = [];
}

public class StaffDashboardDto
{
    public string FullName { get; set; } = string.Empty;
    public int TodayAppointmentCount { get; set; }
    public int PendingAppointmentCount { get; set; }
    public int RecentPatientCount { get; set; }
    public IReadOnlyList<AppointmentDto> TodayAppointments { get; set; } = [];
    public IReadOnlyList<AppointmentDto> PendingAppointments { get; set; } = [];
}

public class AdminDashboardDto
{
    public int UserCount { get; set; }
    public int PatientCount { get; set; }
    public int StaffCount { get; set; }
    public int AppointmentCount { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int MedicalRecordCount { get; set; }
}

public class AdminUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
