using HealthVault.Domain.Entities;
using HealthVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthVault.Infrastructure.Persistence;

public class HealthVaultDbContext : IdentityDbContext<ApplicationUser>
{
    public HealthVaultDbContext(DbContextOptions<HealthVaultDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<MedicalRecordCategory> MedicalRecordCategories => Set<MedicalRecordCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(80).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        builder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).HasMaxLength(120).IsRequired();
            entity.Property(d => d.Description).HasMaxLength(400);
            entity.HasIndex(d => d.Name).IsUnique();
        });

        builder.Entity<MedicalRecordCategory>(entity =>
        {
            entity.ToTable("MedicalRecordCategories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Code).HasMaxLength(40).IsRequired();
            entity.Property(c => c.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(c => c.Code).IsUnique();
        });

        builder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staff");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.UserId).HasMaxLength(450).IsRequired();
            entity.Property(s => s.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(s => s.LastName).HasMaxLength(80).IsRequired();
            entity.Property(s => s.EmployeeNumber).HasMaxLength(30);
            entity.Property(s => s.Specialization).HasMaxLength(120);
            entity.Property(s => s.Phone).HasMaxLength(30);
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasIndex(s => s.EmployeeNumber).IsUnique().HasFilter("[EmployeeNumber] IS NOT NULL");
            entity.HasIndex(s => new { s.LastName, s.FirstName });
            entity.HasOne(s => s.Department)
                .WithMany(d => d.StaffMembers)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.UserId).HasMaxLength(450);
            entity.Property(p => p.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(p => p.LastName).HasMaxLength(80).IsRequired();
            entity.Property(p => p.Email).HasMaxLength(256);
            entity.Property(p => p.Phone).HasMaxLength(30).IsRequired();
            entity.Property(p => p.GhanaCardNumber).HasMaxLength(20);
            entity.Property(p => p.AddressLine1).HasMaxLength(200);
            entity.Property(p => p.AddressLine2).HasMaxLength(200);
            entity.Property(p => p.City).HasMaxLength(80);
            entity.Property(p => p.Region).HasMaxLength(80);
            entity.Property(p => p.PostalCode).HasMaxLength(20);
            entity.Property(p => p.Country).HasMaxLength(80).HasDefaultValue("Ghana");
            entity.Property(p => p.EmergencyContactName).HasMaxLength(160);
            entity.Property(p => p.EmergencyContactPhone).HasMaxLength(30);
            entity.Property(p => p.EmergencyContactRelation).HasMaxLength(50);
            entity.Property(p => p.Allergies).HasMaxLength(500);
            entity.Property(p => p.Notes).HasMaxLength(1000);
            entity.Property(p => p.CreatedByUserId).HasMaxLength(450);
            entity.HasIndex(p => p.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            entity.HasIndex(p => p.Email);
            entity.HasIndex(p => p.Phone);
            entity.HasIndex(p => p.GhanaCardNumber);
            entity.HasIndex(p => new { p.LastName, p.FirstName });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments", t =>
            {
                t.HasCheckConstraint("CK_Appointments_Time", "[EndTime] > [StartTime]");
            });
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Reason).HasMaxLength(300).IsRequired();
            entity.Property(a => a.Notes).HasMaxLength(1000);
            entity.Property(a => a.CancellationReason).HasMaxLength(400);
            entity.Property(a => a.CancelledByUserId).HasMaxLength(450);
            entity.Property(a => a.CreatedByUserId).HasMaxLength(450);
            entity.HasIndex(a => a.PatientId);
            entity.HasIndex(a => a.StaffId);
            entity.HasIndex(a => a.AppointmentDate);
            entity.HasIndex(a => a.Status);
            entity.HasIndex(a => new { a.StaffId, a.AppointmentDate, a.StartTime })
                .IsUnique()
                .HasFilter("[Status] <> 4")
                .HasDatabaseName("UX_Appointments_Staff_Slot");
            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MedicalRecord>(entity =>
        {
            entity.ToTable("MedicalRecords", t =>
            {
                t.HasCheckConstraint("CK_MedicalRecords_Size", "[FileSizeBytes] > 0 AND [FileSizeBytes] <= 10485760");
            });
            entity.HasKey(r => r.Id);
            entity.Property(r => r.UploadedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(r => r.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(r => r.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(r => r.ContentType).HasMaxLength(120).IsRequired();
            entity.HasIndex(r => r.PatientId);
            entity.HasIndex(r => r.CreatedAt);
            entity.HasOne(r => r.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Category)
                .WithMany(c => c.Records)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        SeedLookups(builder);
    }

    private static void SeedLookups(ModelBuilder builder)
    {
        builder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "General Practice", Description = "Primary care and walk-in consultations", IsActive = true },
            new Department { Id = 2, Name = "Outpatient", Description = "Scheduled outpatient clinics", IsActive = true },
            new Department { Id = 3, Name = "Laboratory", Description = "Diagnostics and specimen processing", IsActive = true },
            new Department { Id = 4, Name = "Pharmacy", Description = "Dispensary", IsActive = true },
            new Department { Id = 5, Name = "Administration", Description = "Clinic administration", IsActive = true }
        );

        builder.Entity<MedicalRecordCategory>().HasData(
            new MedicalRecordCategory { Id = 1, Code = "GENERAL", Name = "General", IsDefault = true, IsActive = true },
            new MedicalRecordCategory { Id = 2, Code = "LAB", Name = "Laboratory", IsDefault = false, IsActive = true },
            new MedicalRecordCategory { Id = 3, Code = "IMAGING", Name = "Imaging", IsDefault = false, IsActive = true },
            new MedicalRecordCategory { Id = 4, Code = "PRESCRIPTION", Name = "Prescription", IsDefault = false, IsActive = true },
            new MedicalRecordCategory { Id = 5, Code = "DISCHARGE", Name = "Discharge Summary", IsDefault = false, IsActive = true }
        );
    }
}
