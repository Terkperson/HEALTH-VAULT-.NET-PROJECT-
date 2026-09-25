using System.Net.Http.Headers;
using System.Net.Http.Json;
using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.DTOs.Auth;
using HealthVault.Application.DTOs.Dashboard;
using HealthVault.Application.DTOs.Patients;
using HealthVault.Application.DTOs.Records;
using Microsoft.AspNetCore.Components.Forms;

namespace HealthVault.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionState _session;

    public ApiClient(HttpClient http, SessionState session)
    {
        _http = http;
        _session = session;
    }

    private void Attach()
    {
        _http.DefaultRequestHeaders.Authorization = _session.IsAuthenticated
            ? new AuthenticationHeaderValue("Bearer", _session.Token)
            : null;
    }

    private async Task<ApiResponse<T>> Read<T>(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (body is not null) return body;
        }
        catch
        {
            // fall through
        }

        var text = await response.Content.ReadAsStringAsync();
        return ApiResponse<T>.Fail(string.IsNullOrWhiteSpace(text)
            ? $"Request failed ({(int)response.StatusCode})"
            : text);
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        return await Read<AuthResponse>(response);
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        return await Read<AuthResponse>(response);
    }

    public Task<ApiResponse<PatientDashboardDto>> PatientDashboardAsync()
    {
        Attach();
        return Get<PatientDashboardDto>("api/dashboard/patient");
    }

    public Task<ApiResponse<StaffDashboardDto>> StaffDashboardAsync()
    {
        Attach();
        return Get<StaffDashboardDto>("api/dashboard/staff");
    }

    public Task<ApiResponse<AdminDashboardDto>> AdminDashboardAsync()
    {
        Attach();
        return Get<AdminDashboardDto>("api/dashboard/admin");
    }

    public Task<ApiResponse<IReadOnlyList<AdminUserDto>>> AdminUsersAsync()
    {
        Attach();
        return Get<IReadOnlyList<AdminUserDto>>("api/admin/users");
    }

    public Task<ApiResponse<PagedResult<PatientDto>>> SearchPatientsAsync(string? query, int page = 1)
    {
        Attach();
        var q = string.IsNullOrWhiteSpace(query) ? "" : $"&query={Uri.EscapeDataString(query)}";
        return Get<PagedResult<PatientDto>>($"api/patients?page={page}&pageSize=20{q}");
    }

    public Task<ApiResponse<PatientDto>> GetPatientAsync(Guid id)
    {
        Attach();
        return Get<PatientDto>($"api/patients/{id}");
    }

    public Task<ApiResponse<PatientDto>> GetMyProfileAsync()
    {
        Attach();
        return Get<PatientDto>("api/patients/me");
    }

    public Task<ApiResponse<PatientDto>> CreatePatientAsync(UpsertPatientRequest request)
    {
        Attach();
        return Post("api/patients", request);
    }

    public Task<ApiResponse<PatientDto>> UpdatePatientAsync(Guid id, UpsertPatientRequest request)
    {
        Attach();
        return Put($"api/patients/{id}", request);
    }

    public Task<ApiResponse<IReadOnlyList<AppointmentDto>>> ListAppointmentsAsync(Guid? patientId = null, byte? status = null, DateOnly? date = null)
    {
        Attach();
        var parts = new List<string>();
        if (patientId.HasValue) parts.Add($"patientId={patientId}");
        if (status.HasValue) parts.Add($"status={status}");
        if (date.HasValue) parts.Add($"date={date:yyyy-MM-dd}");
        var qs = parts.Count == 0 ? "" : "?" + string.Join("&", parts);
        return Get<IReadOnlyList<AppointmentDto>>($"api/appointments{qs}");
    }

    public Task<ApiResponse<IReadOnlyList<StaffOptionDto>>> ListStaffAsync()
    {
        Attach();
        return Get<IReadOnlyList<StaffOptionDto>>("api/appointments/staff");
    }

    public Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        Attach();
        return Post("api/appointments", request);
    }

    public Task<ApiResponse<AppointmentDto>> UpdateStatusAsync(Guid id, byte status, string? reason = null)
    {
        Attach();
        return Put($"api/appointments/{id}/status", new UpdateAppointmentStatusRequest { Status = status, CancellationReason = reason });
    }

    public Task<ApiResponse<AppointmentDto>> CancelAppointmentAsync(Guid id, string reason)
    {
        Attach();
        return Put($"api/appointments/{id}/cancel", new CancelAppointmentRequest { CancellationReason = reason });
    }

    public Task<ApiResponse<IReadOnlyList<MedicalRecordDto>>> ListRecordsAsync(Guid patientId)
    {
        Attach();
        return Get<IReadOnlyList<MedicalRecordDto>>($"api/medicalrecords/patient/{patientId}");
    }

    public async Task<ApiResponse<MedicalRecordDto>> UploadRecordAsync(Guid patientId, string title, string? description, IBrowserFile file)
    {
        Attach();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(title), "title");
        if (!string.IsNullOrWhiteSpace(description))
            content.Add(new StringContent(description), "description");

        var stream = file.OpenReadStream(10 * 1024 * 1024);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var response = await _http.PostAsync($"api/medicalrecords/patient/{patientId}", content);
        return await Read<MedicalRecordDto>(response);
    }

    public async Task<(byte[]? Bytes, string FileName, string ContentType, string? Error)> DownloadRecordAsync(Guid id, string fileName)
    {
        Attach();
        var response = await _http.GetAsync($"api/medicalrecords/{id}/download");
        if (!response.IsSuccessStatusCode)
        {
            return (null, fileName, "application/octet-stream", "Download failed.");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var type = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var name = response.Content.Headers.ContentDisposition?.FileNameStar
                   ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                   ?? fileName;
        return (bytes, name, type, null);
    }

    private async Task<ApiResponse<T>> Get<T>(string url)
    {
        var response = await _http.GetAsync(url);
        return await Read<T>(response);
    }

    private async Task<ApiResponse<PatientDto>> Post(string url, UpsertPatientRequest body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        return await Read<PatientDto>(response);
    }

    private async Task<ApiResponse<AppointmentDto>> Post(string url, CreateAppointmentRequest body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        return await Read<AppointmentDto>(response);
    }

    private async Task<ApiResponse<PatientDto>> Put(string url, UpsertPatientRequest body)
    {
        var response = await _http.PutAsJsonAsync(url, body);
        return await Read<PatientDto>(response);
    }

    private async Task<ApiResponse<AppointmentDto>> Put(string url, UpdateAppointmentStatusRequest body)
    {
        var response = await _http.PutAsJsonAsync(url, body);
        return await Read<AppointmentDto>(response);
    }

    private async Task<ApiResponse<AppointmentDto>> Put(string url, CancelAppointmentRequest body)
    {
        var response = await _http.PutAsJsonAsync(url, body);
        return await Read<AppointmentDto>(response);
    }
}