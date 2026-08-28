/*
    HealthVault — useful SSMS / workbench queries
*/
USE HealthVaultDb;
GO

-- Users and their current role (admin dashboard equivalent)
SELECT
    u.Email,
    u.FirstName + N' ' + u.LastName AS FullName,
    u.PhoneNumber,
    r.Name AS Role,
    u.IsActive,
    u.CreatedAt
FROM dbo.AspNetUsers u
LEFT JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
ORDER BY r.Name, u.LastName;

-- Patient directory
SELECT * FROM dbo.vw_PatientDirectory ORDER BY LastName, FirstName;

-- Today's appointment board
SELECT * FROM dbo.vw_AppointmentBoard
WHERE AppointmentDate = CONVERT(date, SYSUTCDATETIME())
ORDER BY StartTime;

-- Double-booking check for a clinician
DECLARE @StaffId UNIQUEIDENTIFIER = NULL; -- paste a Staff.Id
DECLARE @Date DATE = CONVERT(date, SYSUTCDATETIME());

SELECT AppointmentDate, StartTime, EndTime, Status, PatientId
FROM dbo.Appointments
WHERE StaffId = @StaffId
  AND AppointmentDate = @Date
  AND Status <> 4
ORDER BY StartTime;

-- Records uploaded for a patient
DECLARE @PatientId UNIQUEIDENTIFIER = NULL; -- paste a Patients.Id
SELECT r.Title, c.Name AS Category, r.OriginalFileName, r.FileSizeBytes, r.CreatedAt
FROM dbo.MedicalRecords r
JOIN dbo.MedicalRecordCategories c ON c.Id = r.CategoryId
WHERE r.PatientId = @PatientId
ORDER BY r.CreatedAt DESC;

-- Admin counts
SELECT
    (SELECT COUNT(*) FROM dbo.AspNetUsers)     AS Users,
    (SELECT COUNT(*) FROM dbo.Patients WHERE IsActive = 1) AS Patients,
    (SELECT COUNT(*) FROM dbo.Staff WHERE IsActive = 1)    AS Staff,
    (SELECT COUNT(*) FROM dbo.Appointments)    AS Appointments,
    (SELECT COUNT(*) FROM dbo.MedicalRecords)  AS Records;
GO
