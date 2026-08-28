using HealthVault.Domain.Entities;
using HealthVault.Domain.Enums;
using HealthVault.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthVault.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        HealthVaultDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles,
        ILogger logger)
    {
        // Prefer EnsureCreated so a database built from the SSMS scripts
        // (already has tables) is left alone, while a fresh empty database
        // still gets the EF model.
        await db.Database.EnsureCreatedAsync();

        foreach (var role in UserRoles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        await EnsureUserAsync(users, db, logger,
            email: "admin@healthvault.com",
            password: "Admin@12345",
            firstName: "Ama",
            lastName: "Mensah",
            phone: "+233201000001",
            role: UserRoles.Administrator,
            staff: new Staff
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                FirstName = "Ama",
                LastName = "Mensah",
                EmployeeNumber = "HV-ADM-001",
                Specialization = "Clinic Administrator",
                Phone = "+233201000001",
                DepartmentId = 5,
                HireDate = new DateOnly(2024, 1, 8),
                IsActive = true
            });

        await EnsureUserAsync(users, db, logger,
            email: "staff@healthvault.com",
            password: "Staff@12345",
            firstName: "Kwame",
            lastName: "Boateng",
            phone: "+233201000002",
            role: UserRoles.Staff,
            staff: new Staff
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                FirstName = "Kwame",
                LastName = "Boateng",
                EmployeeNumber = "HV-STF-001",
                Specialization = "General Practitioner",
                Phone = "+233201000002",
                DepartmentId = 1,
                HireDate = new DateOnly(2024, 3, 1),
                IsActive = true
            });

        await EnsureUserAsync(users, db, logger,
            email: "nurse@healthvault.com",
            password: "Staff@12345",
            firstName: "Efua",
            lastName: "Owusu",
            phone: "+233201000003",
            role: UserRoles.Staff,
            staff: new Staff
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                FirstName = "Efua",
                LastName = "Owusu",
                EmployeeNumber = "HV-STF-002",
                Specialization = "Registered Nurse",
                Phone = "+233201000003",
                DepartmentId = 2,
                HireDate = new DateOnly(2024, 6, 15),
                IsActive = true
            });

        var demoPatient = await users.FindByEmailAsync("patient@healthvault.com");
        if (demoPatient is null)
        {
            demoPatient = new ApplicationUser
            {
                UserName = "patient@healthvault.com",
                Email = "patient@healthvault.com",
                EmailConfirmed = true,
                FirstName = "Kojo",
                LastName = "Asante",
                PhoneNumber = "+233244000100",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await users.CreateAsync(demoPatient, "Patient@12345");
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed demo patient: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                await users.AddToRoleAsync(demoPatient, UserRoles.Patient);
            }
        }

        if (demoPatient is not null && !await db.Patients.AnyAsync(p => p.UserId == demoPatient.Id))
        {
            db.Patients.Add(new Patient
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                UserId = demoPatient.Id,
                FirstName = "Kojo",
                LastName = "Asante",
                DateOfBirth = new DateOnly(1994, 5, 12),
                Gender = Gender.Male,
                Email = "patient@healthvault.com",
                Phone = "+233244000100",
                GhanaCardNumber = "GHA-123456789-1",
                BloodGroup = BloodGroup.OPositive,
                AddressLine1 = "12 Liberation Road",
                City = "Medina Estates",
                Region = "Greater Accra",
                Country = "Ghana",
                EmergencyContactName = "Abena Asante",
                EmergencyContactPhone = "+233244000101",
                EmergencyContactRelation = "Spouse",
                Allergies = "Penicillin",
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> users,
        HealthVaultDbContext db,
        ILogger logger,
        string email,
        string password,
        string firstName,
        string lastName,
        string phone,
        string role,
        Staff staff)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed {Email}: {Errors}", email, string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
            await users.AddToRoleAsync(user, role);
        }
        else if (!await users.IsInRoleAsync(user, role))
        {
            await users.AddToRoleAsync(user, role);
        }

        if (!await db.StaffMembers.AnyAsync(s => s.UserId == user.Id))
        {
            staff.UserId = user.Id;
            staff.CreatedAt = DateTime.UtcNow;
            db.StaffMembers.Add(staff);
            await db.SaveChangesAsync();
        }
    }
}
