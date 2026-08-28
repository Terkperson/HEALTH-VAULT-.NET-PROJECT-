using HealthVault.Domain.Common;

namespace HealthVault.Domain.Entities;

public class Staff : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public DateOnly? HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public Department? Department { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
