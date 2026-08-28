using HealthVault.Domain.Common;

namespace HealthVault.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public Patient Patient { get; set; } = null!;
    public MedicalRecordCategory Category { get; set; } = null!;
}
