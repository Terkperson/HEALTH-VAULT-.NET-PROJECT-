using HealthVault.Domain.Common;
using HealthVault.Domain.Enums;

namespace HealthVault.Domain.Entities;

public class Patient : BaseEntity
{
    public string? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.PreferNotToSay;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? GhanaCardNumber { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Ghana";
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Allergies { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedByUserId { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
