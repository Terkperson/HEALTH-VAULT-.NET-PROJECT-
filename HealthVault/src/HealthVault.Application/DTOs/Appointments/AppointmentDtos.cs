using System.ComponentModel.DataAnnotations;

namespace HealthVault.Application.DTOs.Appointments;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? StaffSpecialization { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public byte Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentRequest
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    [Required, StringLength(300, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    [Required, StringLength(300, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    [Required]
    public byte Status { get; set; }

    [StringLength(400)]
    public string? CancellationReason { get; set; }
}

public class CancelAppointmentRequest
{
    [Required, StringLength(400, MinimumLength = 3)]
    public string CancellationReason { get; set; } = string.Empty;
}

public class StaffOptionDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? Department { get; set; }
}
