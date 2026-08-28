using HealthVault.Application.Common;
using HealthVault.Application.DTOs.Records;
using HealthVault.Application.Interfaces;
using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Mapping;
using HealthVault.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace HealthVault.Infrastructure.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly HealthVaultDbContext _db;
    private readonly string _uploadRoot;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".docx"
    };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    private const long MaxBytes = 10 * 1024 * 1024;

    public MedicalRecordService(HealthVaultDbContext db, IHostEnvironment env)
    {
        _db = db;
        _uploadRoot = Path.Combine(env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<ApiResponse<IReadOnlyList<MedicalRecordDto>>> ListForPatientAsync(
        Guid patientId, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var patient = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient is null) return ApiResponse<IReadOnlyList<MedicalRecordDto>>.Fail("Patient not found.");
        if (!CanAccessPatient(patient, actorUserId, roles))
            return ApiResponse<IReadOnlyList<MedicalRecordDto>>.Fail("You are not allowed to view these records.");

        var records = await _db.MedicalRecords.AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Category)
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return ApiResponse<IReadOnlyList<MedicalRecordDto>>.Ok(records.Select(r => r.ToDto()).ToList());
    }

    public async Task<ApiResponse<MedicalRecordDto>> GetByIdAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var record = await LoadAsync(id, ct);
        if (record is null) return ApiResponse<MedicalRecordDto>.Fail("Record not found.");
        if (!CanAccessPatient(record.Patient, actorUserId, roles))
            return ApiResponse<MedicalRecordDto>.Fail("You are not allowed to view this record.");
        return ApiResponse<MedicalRecordDto>.Ok(record.ToDto());
    }

    public async Task<ApiResponse<MedicalRecordDto>> UploadAsync(
        Guid patientId,
        string title,
        string? description,
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSize,
        string uploadedByUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return ApiResponse<MedicalRecordDto>.Fail("A title is required.");

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId, ct);
        if (patient is null) return ApiResponse<MedicalRecordDto>.Fail("Patient not found.");
        if (!CanUpload(patient, uploadedByUserId, roles))
            return ApiResponse<MedicalRecordDto>.Fail("You are not allowed to upload records for this patient.");

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
            return ApiResponse<MedicalRecordDto>.Fail("Only PDF, JPG, PNG and DOCX files are allowed.");
        if (!AllowedContentTypes.Contains(contentType) && !AllowedExtensions.Contains(extension))
            return ApiResponse<MedicalRecordDto>.Fail("Unsupported file type.");
        if (fileSize <= 0 || fileSize > MaxBytes)
            return ApiResponse<MedicalRecordDto>.Fail("File must be between 1 byte and 10 MB.");

        var category = await _db.MedicalRecordCategories.FirstOrDefaultAsync(c => c.IsDefault, ct)
                       ?? await _db.MedicalRecordCategories.FirstAsync(ct);

        var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var relativePath = Path.Combine("App_Data", "uploads", storedName);
        var fullPath = Path.Combine(_uploadRoot, storedName);

        await using (var output = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(output, ct);
        }

        var record = new MedicalRecord
        {
            PatientId = patient.Id,
            UploadedByUserId = uploadedByUserId,
            CategoryId = category.Id,
            Title = title.Trim(),
            Description = description?.Trim(),
            OriginalFileName = Path.GetFileName(originalFileName),
            StoredFileName = storedName,
            FilePath = relativePath,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSizeBytes = fileSize
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync(ct);
        record.Patient = patient;
        record.Category = category;
        return ApiResponse<MedicalRecordDto>.Ok(record.ToDto(), "Record uploaded.");
    }

    public async Task<ApiResponse<FileDownload>> DownloadAsync(Guid id, string actorUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var record = await LoadAsync(id, ct);
        if (record is null) return ApiResponse<FileDownload>.Fail("Record not found.");
        if (!CanAccessPatient(record.Patient, actorUserId, roles))
            return ApiResponse<FileDownload>.Fail("You are not allowed to download this record.");

        var fullPath = Path.Combine(_uploadRoot, record.StoredFileName);
        if (!File.Exists(fullPath))
            return ApiResponse<FileDownload>.Fail("The file is no longer available on the server.");

        return ApiResponse<FileDownload>.Ok(new FileDownload
        {
            Content = File.OpenRead(fullPath),
            ContentType = record.ContentType,
            FileName = record.OriginalFileName
        });
    }

    private async Task<MedicalRecord?> LoadAsync(Guid id, CancellationToken ct) =>
        await _db.MedicalRecords
            .Include(r => r.Patient)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    private static bool CanAccessPatient(Patient patient, string actorUserId, IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator)) return true;
        return patient.UserId == actorUserId;
    }

    private static bool CanUpload(Patient patient, string actorUserId, IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (roleSet.Contains(UserRoles.Staff) || roleSet.Contains(UserRoles.Administrator)) return true;
        return patient.UserId == actorUserId;
    }
}
