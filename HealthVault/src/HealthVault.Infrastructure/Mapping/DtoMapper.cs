using HealthVault.Application.DTOs.Appointments;
using HealthVault.Application.DTOs.Patients;
using HealthVault.Application.DTOs.Records;
using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;

namespace HealthVault.Infrastructure.Mapping;

public static class DtoMapper
{
    public static string GenderName(Gender gender) => gender switch
    {
        Gender.Female => "Female",
        Gender.Male => "Male",
        Gender.Other => "Other",
        _ => "Prefer not to say"
    };

    public static string BloodGroupName(BloodGroup? group) => group switch
    {
        BloodGroup.APositive => "A+",
        BloodGroup.ANegative => "A-",
        BloodGroup.BPositive => "B+",
        BloodGroup.BNegative => "B-",
        BloodGroup.ABPositive => "AB+",
        BloodGroup.ABNegative => "AB-",
        BloodGroup.OPositive => "O+",
        BloodGroup.ONegative => "O-",
        BloodGroup.Unknown => "Unknown",
        _ => string.Empty
    };

    public static string StatusName(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "Pending",
        AppointmentStatus.Confirmed => "Confirmed",
        AppointmentStatus.Completed => "Completed",
        AppointmentStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    public static string FileSizeDisplay(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.#} KB";
        return $"{bytes / (1024.0 * 1024.0):0.##} MB";
    }

    public static PatientDto ToDto(this Patient p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        FirstName = p.FirstName,
        LastName = p.LastName,
        FullName = p.FullName,
        DateOfBirth = p.DateOfBirth,
        Age = p.Age,
        Gender = (byte)p.Gender,
        GenderName = GenderName(p.Gender),
        Email = p.Email,
        Phone = p.Phone,
        GhanaCardNumber = p.GhanaCardNumber,
        BloodGroup = p.BloodGroup is null ? null : (byte)p.BloodGroup,
        BloodGroupName = BloodGroupName(p.BloodGroup),
        AddressLine1 = p.AddressLine1,
        AddressLine2 = p.AddressLine2,
        City = p.City,
        Region = p.Region,
        PostalCode = p.PostalCode,
        Country = p.Country,
        EmergencyContactName = p.EmergencyContactName,
        EmergencyContactPhone = p.EmergencyContactPhone,
        EmergencyContactRelation = p.EmergencyContactRelation,
        Allergies = p.Allergies,
        Notes = p.Notes,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };

    public static AppointmentDto ToDto(this Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        PatientName = a.Patient?.FullName ?? string.Empty,
        PatientPhone = a.Patient?.Phone,
        StaffId = a.StaffId,
        StaffName = a.Staff?.FullName ?? string.Empty,
        StaffSpecialization = a.Staff?.Specialization,
        AppointmentDate = a.AppointmentDate,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Status = (byte)a.Status,
        StatusName = StatusName(a.Status),
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CancelledAt = a.CancelledAt,
        CreatedAt = a.CreatedAt
    };

    public static MedicalRecordDto ToDto(this MedicalRecord r) => new()
    {
        Id = r.Id,
        PatientId = r.PatientId,
        PatientName = r.Patient?.FullName ?? string.Empty,
        CategoryId = r.CategoryId,
        CategoryName = r.Category?.Name ?? "General",
        Title = r.Title,
        Description = r.Description,
        OriginalFileName = r.OriginalFileName,
        ContentType = r.ContentType,
        FileSizeBytes = r.FileSizeBytes,
        FileSizeDisplay = FileSizeDisplay(r.FileSizeBytes),
        UploadedAt = r.CreatedAt,
        UploadedByUserId = r.UploadedByUserId
    };
}
