using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.DTOs.Auth;
using HealthVault.Application.DTOs.Dashboard;
using HealthVault.Application.DTOs.Patients;
using HealthVault.Application.DTOs.Records;

namespace HealthVault.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public interface IPatientService
{
    Task<ApiResponse<PagedResult<PatientDto>>> SearchAsync(PatientSearchRequest request, CancellationToken ct = default);
    Task<ApiResponse<PatientDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PatientDto>> GetMineAsync(string userId, CancellationToken ct = default);
    Task<ApiResponse<PatientDto>> CreateAsync(UpsertPatientRequest request, string createdByUserId, CancellationToken ct = default);
    Task<ApiResponse<PatientDto>> UpdateAsync(Guid id, UpsertPatientRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
}

public interface IAppointmentService
{
    Task<ApiResponse<IReadOnlyList<AppointmentDto>>> ListAsync(string actorUserId, IEnumerable<string> roles, Guid? patientId, byte? status, DateOnly? date, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> GetByIdAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> CreateAsync(CreateAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> UpdateAsync(Guid id, UpdateAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<AppointmentDto>> CancelAsync(Guid id, CancelAppointmentRequest request, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<StaffOptionDto>>> ListStaffAsync(CancellationToken ct = default);
}

public interface IMedicalRecordService
{
    Task<ApiResponse<IReadOnlyList<MedicalRecordDto>>> ListForPatientAsync(Guid patientId, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<MedicalRecordDto>> GetByIdAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
    Task<ApiResponse<MedicalRecordDto>> UploadAsync(
        Guid patientId,
        string title,
        string? description,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSize,
        string uploadedByUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default);
    Task<ApiResponse<FileDownload>> DownloadAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<ApiResponse<PatientDashboardDto>> GetPatientAsync(string userId, CancellationToken ct = default);
    Task<ApiResponse<StaffDashboardDto>> GetStaffAsync(string userId, CancellationToken ct = default);
    Task<ApiResponse<AdminDashboardDto>> GetAdminAsync(CancellationToken ct = default);
}

public interface IAdminService
{
    Task<ApiResponse<IReadOnlyList<AdminUserDto>>> ListUsersAsync(CancellationToken ct = default);
}
