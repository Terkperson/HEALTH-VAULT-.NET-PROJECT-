namespace HealthVault.Domain.Entities;

public class MedicalRecordCategory
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MedicalRecord> Records { get; set; } = new List<MedicalRecord>();
}
