/*
    HealthVault — lookup + role seed
    Script 03: safe to re-run (idempotent)

    Login accounts (password hashes) are created by the .NET seeder
    on first API start, because Identity must hash the password.
    Demo accounts after first run:

        admin@healthvault.com     Admin@12345     Administrator
        staff@healthvault.com     Staff@12345     Staff
        nurse@healthvault.com     Staff@12345     Staff
        patient@healthvault.com   Patient@12345   Patient
*/
USE HealthVaultDb;
GO

SET NOCOUNT ON;

MERGE dbo.Genders AS t
USING (VALUES
    (1, N'FEMALE',            N'Female'),
    (2, N'MALE',              N'Male'),
    (3, N'OTHER',             N'Other'),
    (4, N'PREFER_NOT_TO_SAY', N'Prefer not to say')
) AS s (GenderId, Code, Name)
ON t.GenderId = s.GenderId
WHEN MATCHED THEN UPDATE SET Code = s.Code, Name = s.Name
WHEN NOT MATCHED THEN INSERT (GenderId, Code, Name) VALUES (s.GenderId, s.Code, s.Name);

MERGE dbo.BloodGroups AS t
USING (VALUES
    (1, N'A+',  N'A+'),
    (2, N'A-',  N'A-'),
    (3, N'B+',  N'B+'),
    (4, N'B-',  N'B-'),
    (5, N'AB+', N'AB+'),
    (6, N'AB-', N'AB-'),
    (7, N'O+',  N'O+'),
    (8, N'O-',  N'O-'),
    (9, N'UNK', N'Unknown')
) AS s (BloodGroupId, Code, Name)
ON t.BloodGroupId = s.BloodGroupId
WHEN MATCHED THEN UPDATE SET Code = s.Code, Name = s.Name
WHEN NOT MATCHED THEN INSERT (BloodGroupId, Code, Name) VALUES (s.BloodGroupId, s.Code, s.Name);

MERGE dbo.AppointmentStatuses AS t
USING (VALUES
    (1, N'PENDING',   N'Pending',   1),
    (2, N'CONFIRMED', N'Confirmed', 2),
    (3, N'COMPLETED', N'Completed', 3),
    (4, N'CANCELLED', N'Cancelled', 4)
) AS s (StatusId, Code, Name, SortOrder)
ON t.StatusId = s.StatusId
WHEN MATCHED THEN UPDATE SET Code = s.Code, Name = s.Name, SortOrder = s.SortOrder
WHEN NOT MATCHED THEN INSERT (StatusId, Code, Name, SortOrder) VALUES (s.StatusId, s.Code, s.Name, s.SortOrder);

IF NOT EXISTS (SELECT 1 FROM dbo.Departments)
BEGIN
    INSERT INTO dbo.Departments (Name, Description, IsActive) VALUES
        (N'General Practice', N'Primary care and walk-in consultations', 1),
        (N'Outpatient',       N'Scheduled outpatient clinics',           1),
        (N'Laboratory',       N'Diagnostics and specimen processing',    1),
        (N'Pharmacy',         N'Dispensary',                             1),
        (N'Administration',   N'Clinic administration',                  1);
END

IF NOT EXISTS (SELECT 1 FROM dbo.MedicalRecordCategories)
BEGIN
    SET IDENTITY_INSERT dbo.MedicalRecordCategories ON;
    INSERT INTO dbo.MedicalRecordCategories (Id, Code, Name, IsDefault, IsActive) VALUES
        (1, N'GENERAL',      N'General',           1, 1),
        (2, N'LAB',          N'Laboratory',        0, 1),
        (3, N'IMAGING',      N'Imaging',           0, 1),
        (4, N'PRESCRIPTION', N'Prescription',      0, 1),
        (5, N'DISCHARGE',    N'Discharge Summary', 0, 1);
    SET IDENTITY_INSERT dbo.MedicalRecordCategories OFF;
END

/* Roles — Identity also seeds these on API start. Safe either way. */
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'ADMINISTRATOR')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (N'role-administrator', N'Administrator', N'ADMINISTRATOR', CONVERT(nvarchar(36), NEWID()));

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'STAFF')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (N'role-staff', N'Staff', N'STAFF', CONVERT(nvarchar(36), NEWID()));

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = N'PATIENT')
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (N'role-patient', N'Patient', N'PATIENT', CONVERT(nvarchar(36), NEWID()));

PRINT N'Lookup data and roles seeded.';
GO
