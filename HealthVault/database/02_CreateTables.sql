/*
    HealthVault — full relational schema
    Script 02: tables, keys, checks, indexes
    Compatible with ASP.NET Core Identity + EF Core 8

    Run AFTER 01_CreateDatabase.sql
    Target: Microsoft SQL Server 2019+ / Azure SQL
*/
USE HealthVaultDb;
GO

/* ------------------------------------------------------------------
   1. LOOKUP TABLES
   ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Genders', N'U') IS NULL
CREATE TABLE dbo.Genders
(
    GenderId    TINYINT         NOT NULL,
    Code        NVARCHAR(20)    NOT NULL,
    Name        NVARCHAR(50)    NOT NULL,
    CONSTRAINT PK_Genders PRIMARY KEY (GenderId),
    CONSTRAINT UQ_Genders_Code UNIQUE (Code)
);

IF OBJECT_ID(N'dbo.BloodGroups', N'U') IS NULL
CREATE TABLE dbo.BloodGroups
(
    BloodGroupId TINYINT        NOT NULL,
    Code         NVARCHAR(8)    NOT NULL,
    Name         NVARCHAR(20)   NOT NULL,
    CONSTRAINT PK_BloodGroups PRIMARY KEY (BloodGroupId),
    CONSTRAINT UQ_BloodGroups_Code UNIQUE (Code)
);

IF OBJECT_ID(N'dbo.AppointmentStatuses', N'U') IS NULL
CREATE TABLE dbo.AppointmentStatuses
(
    StatusId    TINYINT         NOT NULL,
    Code        NVARCHAR(20)    NOT NULL,
    Name        NVARCHAR(40)    NOT NULL,
    SortOrder   TINYINT         NOT NULL,
    CONSTRAINT PK_AppointmentStatuses PRIMARY KEY (StatusId),
    CONSTRAINT UQ_AppointmentStatuses_Code UNIQUE (Code)
);

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
CREATE TABLE dbo.Departments
(
    Id          INT             IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(120)   NOT NULL,
    Description NVARCHAR(400)   NULL,
    IsActive    BIT             NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT (1),
    CONSTRAINT PK_Departments PRIMARY KEY (Id),
    CONSTRAINT UQ_Departments_Name UNIQUE (Name)
);

IF OBJECT_ID(N'dbo.MedicalRecordCategories', N'U') IS NULL
CREATE TABLE dbo.MedicalRecordCategories
(
    Id          INT             IDENTITY(1,1) NOT NULL,
    Code        NVARCHAR(40)    NOT NULL,
    Name        NVARCHAR(80)    NOT NULL,
    IsDefault   BIT             NOT NULL CONSTRAINT DF_MRC_IsDefault DEFAULT (0),
    IsActive    BIT             NOT NULL CONSTRAINT DF_MRC_IsActive DEFAULT (1),
    CONSTRAINT PK_MedicalRecordCategories PRIMARY KEY (Id),
    CONSTRAINT UQ_MedicalRecordCategories_Code UNIQUE (Code)
);
GO

/* ------------------------------------------------------------------
   2. ASP.NET CORE IDENTITY
   Extra columns on AspNetUsers: FirstName, LastName, IsActive,
   CreatedAt, UpdatedAt — used by HealthVault profiles.
   ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
CREATE TABLE dbo.AspNetRoles
(
    Id               NVARCHAR(450)  NOT NULL,
    Name             NVARCHAR(256)  NULL,
    NormalizedName   NVARCHAR(256)  NULL,
    ConcurrencyStamp NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
CREATE TABLE dbo.AspNetUsers
(
    Id                   NVARCHAR(450)  NOT NULL,
    UserName             NVARCHAR(256)  NULL,
    NormalizedUserName   NVARCHAR(256)  NULL,
    Email                NVARCHAR(256)  NULL,
    NormalizedEmail      NVARCHAR(256)  NULL,
    EmailConfirmed       BIT            NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT (0),
    PasswordHash         NVARCHAR(MAX)  NULL,
    SecurityStamp        NVARCHAR(MAX)  NULL,
    ConcurrencyStamp     NVARCHAR(MAX)  NULL,
    PhoneNumber          NVARCHAR(MAX)  NULL,
    PhoneNumberConfirmed BIT            NOT NULL CONSTRAINT DF_Users_PhoneConfirmed DEFAULT (0),
    TwoFactorEnabled     BIT            NOT NULL CONSTRAINT DF_Users_TwoFactor DEFAULT (0),
    LockoutEnd           DATETIMEOFFSET NULL,
    LockoutEnabled       BIT            NOT NULL CONSTRAINT DF_Users_Lockout DEFAULT (1),
    AccessFailedCount    INT            NOT NULL CONSTRAINT DF_Users_AccessFailed DEFAULT (0),
    FirstName            NVARCHAR(80)   NOT NULL,
    LastName             NVARCHAR(80)   NOT NULL,
    IsActive             BIT            NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt            DATETIME2(7)   NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt            DATETIME2(7)   NULL,
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
);

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
CREATE TABLE dbo.AspNetRoleClaims
(
    Id         INT            IDENTITY(1,1) NOT NULL,
    RoleId     NVARCHAR(450)  NOT NULL,
    ClaimType  NVARCHAR(MAX)  NULL,
    ClaimValue NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
CREATE TABLE dbo.AspNetUserClaims
(
    Id         INT            IDENTITY(1,1) NOT NULL,
    UserId     NVARCHAR(450)  NOT NULL,
    ClaimType  NVARCHAR(MAX)  NULL,
    ClaimValue NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
CREATE TABLE dbo.AspNetUserLogins
(
    LoginProvider       NVARCHAR(128) NOT NULL,
    ProviderKey         NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId              NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
CREATE TABLE dbo.AspNetUserRoles
(
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles (Id) ON DELETE CASCADE
);

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
CREATE TABLE dbo.AspNetUserTokens
(
    UserId        NVARCHAR(450)  NOT NULL,
    LoginProvider NVARCHAR(128)  NOT NULL,
    Name          NVARCHAR(128)  NOT NULL,
    Value         NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE CASCADE
);
GO

/* ------------------------------------------------------------------
   3. CLINIC DOMAIN
   ------------------------------------------------------------------ */

IF OBJECT_ID(N'dbo.Staff', N'U') IS NULL
CREATE TABLE dbo.Staff
(
    Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Staff_Id DEFAULT (NEWSEQUENTIALID()),
    UserId          NVARCHAR(450)    NOT NULL,
    DepartmentId    INT              NULL,
    FirstName       NVARCHAR(80)     NOT NULL,
    LastName        NVARCHAR(80)     NOT NULL,
    EmployeeNumber  NVARCHAR(30)     NULL,
    Specialization  NVARCHAR(120)    NULL,
    Phone           NVARCHAR(30)     NULL,
    HireDate        DATE             NULL,
    IsActive        BIT              NOT NULL CONSTRAINT DF_Staff_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(7)     NOT NULL CONSTRAINT DF_Staff_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2(7)     NULL,
    CONSTRAINT PK_Staff PRIMARY KEY (Id),
    CONSTRAINT UQ_Staff_UserId UNIQUE (UserId),
    CONSTRAINT FK_Staff_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id),
    CONSTRAINT FK_Staff_Departments
        FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments (Id) ON DELETE SET NULL
);

IF OBJECT_ID(N'dbo.Patients', N'U') IS NULL
CREATE TABLE dbo.Patients
(
    Id                         UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Patients_Id DEFAULT (NEWSEQUENTIALID()),
    UserId                     NVARCHAR(450)    NULL,
    FirstName                  NVARCHAR(80)     NOT NULL,
    LastName                   NVARCHAR(80)     NOT NULL,
    DateOfBirth                DATE             NOT NULL,
    Gender                     TINYINT          NOT NULL,
    Email                      NVARCHAR(256)    NULL,
    Phone                      NVARCHAR(30)     NOT NULL,
    GhanaCardNumber            NVARCHAR(20)     NULL,
    BloodGroup                 TINYINT          NULL,
    AddressLine1               NVARCHAR(200)    NULL,
    AddressLine2               NVARCHAR(200)    NULL,
    City                       NVARCHAR(80)     NULL,
    Region                     NVARCHAR(80)    NULL,
    PostalCode                 NVARCHAR(20)     NULL,
    Country                    NVARCHAR(80)     NOT NULL CONSTRAINT DF_Patients_Country DEFAULT (N'Ghana'),
    EmergencyContactName       NVARCHAR(160)    NULL,
    EmergencyContactPhone      NVARCHAR(30)     NULL,
    EmergencyContactRelation   NVARCHAR(50)     NULL,
    Allergies                  NVARCHAR(500)    NULL,
    Notes                      NVARCHAR(1000)   NULL,
    IsActive                   BIT              NOT NULL CONSTRAINT DF_Patients_IsActive DEFAULT (1),
    CreatedAt                  DATETIME2(7)     NOT NULL CONSTRAINT DF_Patients_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt                  DATETIME2(7)     NULL,
    CreatedByUserId            NVARCHAR(450)    NULL,
    CONSTRAINT PK_Patients PRIMARY KEY (Id),
    CONSTRAINT FK_Patients_Users
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Patients_Genders
        FOREIGN KEY (Gender) REFERENCES dbo.Genders (GenderId),
    CONSTRAINT FK_Patients_BloodGroups
        FOREIGN KEY (BloodGroup) REFERENCES dbo.BloodGroups (BloodGroupId),
    CONSTRAINT CK_Patients_DateOfBirth CHECK (DateOfBirth <= CONVERT(date, SYSUTCDATETIME()))
);

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NULL
CREATE TABLE dbo.Appointments
(
    Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Appointments_Id DEFAULT (NEWSEQUENTIALID()),
    PatientId            UNIQUEIDENTIFIER NOT NULL,
    StaffId              UNIQUEIDENTIFIER NOT NULL,
    AppointmentDate      DATE             NOT NULL,
    StartTime            TIME(0)          NOT NULL,
    EndTime              TIME(0)          NOT NULL,
    Status               TINYINT          NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT (1),
    Reason               NVARCHAR(300)    NOT NULL,
    Notes                NVARCHAR(1000)   NULL,
    CancellationReason   NVARCHAR(400)    NULL,
    CancelledAt          DATETIME2(7)     NULL,
    CancelledByUserId    NVARCHAR(450)    NULL,
    CreatedAt            DATETIME2(7)     NOT NULL CONSTRAINT DF_Appointments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt            DATETIME2(7)     NULL,
    CreatedByUserId      NVARCHAR(450)    NULL,
    CONSTRAINT PK_Appointments PRIMARY KEY (Id),
    CONSTRAINT FK_Appointments_Patients
        FOREIGN KEY (PatientId) REFERENCES dbo.Patients (Id),
    CONSTRAINT FK_Appointments_Staff
        FOREIGN KEY (StaffId) REFERENCES dbo.Staff (Id),
    CONSTRAINT FK_Appointments_Statuses
        FOREIGN KEY (Status) REFERENCES dbo.AppointmentStatuses (StatusId),
    CONSTRAINT CK_Appointments_Time CHECK (EndTime > StartTime)
);

IF OBJECT_ID(N'dbo.MedicalRecords', N'U') IS NULL
CREATE TABLE dbo.MedicalRecords
(
    Id                UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_MedicalRecords_Id DEFAULT (NEWSEQUENTIALID()),
    PatientId         UNIQUEIDENTIFIER NOT NULL,
    UploadedByUserId  NVARCHAR(450)    NOT NULL,
    CategoryId        INT              NOT NULL,
    Title             NVARCHAR(200)    NOT NULL,
    Description       NVARCHAR(1000)   NULL,
    OriginalFileName  NVARCHAR(260)    NOT NULL,
    StoredFileName    NVARCHAR(260)    NOT NULL,
    FilePath          NVARCHAR(500)    NOT NULL,
    ContentType       NVARCHAR(120)    NOT NULL,
    FileSizeBytes     BIGINT           NOT NULL,
    CreatedAt         DATETIME2(7)     NOT NULL CONSTRAINT DF_MedicalRecords_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt         DATETIME2(7)     NULL,
    CONSTRAINT PK_MedicalRecords PRIMARY KEY (Id),
    CONSTRAINT FK_MedicalRecords_Patients
        FOREIGN KEY (PatientId) REFERENCES dbo.Patients (Id),
    CONSTRAINT FK_MedicalRecords_Users
        FOREIGN KEY (UploadedByUserId) REFERENCES dbo.AspNetUsers (Id),
    CONSTRAINT FK_MedicalRecords_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.MedicalRecordCategories (Id),
    CONSTRAINT CK_MedicalRecords_Size CHECK (FileSizeBytes > 0 AND FileSizeBytes <= 10485760)
);
GO

/* ------------------------------------------------------------------
   4. INDEXES  (search, dashboards, double-booking)
   ------------------------------------------------------------------ */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'dbo.AspNetRoles'))
    CREATE UNIQUE INDEX RoleNameIndex ON dbo.AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
    CREATE INDEX EmailIndex ON dbo.AspNetUsers (NormalizedEmail);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'dbo.AspNetUsers'))
    CREATE UNIQUE INDEX UserNameIndex ON dbo.AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'dbo.AspNetRoleClaims'))
    CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims (RoleId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'dbo.AspNetUserClaims'))
    CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims (UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'dbo.AspNetUserLogins'))
    CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins (UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'dbo.AspNetUserRoles'))
    CREATE INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles (RoleId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Staff_EmployeeNumber' AND object_id = OBJECT_ID(N'dbo.Staff'))
    CREATE UNIQUE INDEX UQ_Staff_EmployeeNumber ON dbo.Staff (EmployeeNumber) WHERE EmployeeNumber IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Staff_Name' AND object_id = OBJECT_ID(N'dbo.Staff'))
    CREATE INDEX IX_Staff_Name ON dbo.Staff (LastName, FirstName);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Patients_UserId' AND object_id = OBJECT_ID(N'dbo.Patients'))
    CREATE UNIQUE INDEX UQ_Patients_UserId ON dbo.Patients (UserId) WHERE UserId IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Patients_Email' AND object_id = OBJECT_ID(N'dbo.Patients'))
    CREATE INDEX IX_Patients_Email ON dbo.Patients (Email);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Patients_Phone' AND object_id = OBJECT_ID(N'dbo.Patients'))
    CREATE INDEX IX_Patients_Phone ON dbo.Patients (Phone);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Patients_GhanaCard' AND object_id = OBJECT_ID(N'dbo.Patients'))
    CREATE INDEX IX_Patients_GhanaCard ON dbo.Patients (GhanaCardNumber);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Patients_Name' AND object_id = OBJECT_ID(N'dbo.Patients'))
    CREATE INDEX IX_Patients_Name ON dbo.Patients (LastName, FirstName);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Appointments_PatientId' AND object_id = OBJECT_ID(N'dbo.Appointments'))
    CREATE INDEX IX_Appointments_PatientId ON dbo.Appointments (PatientId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Appointments_StaffId' AND object_id = OBJECT_ID(N'dbo.Appointments'))
    CREATE INDEX IX_Appointments_StaffId ON dbo.Appointments (StaffId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Appointments_Date' AND object_id = OBJECT_ID(N'dbo.Appointments'))
    CREATE INDEX IX_Appointments_Date ON dbo.Appointments (AppointmentDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Appointments_Status' AND object_id = OBJECT_ID(N'dbo.Appointments'))
    CREATE INDEX IX_Appointments_Status ON dbo.Appointments (Status);

/* Application-level double-booking is required by the SRS.
   This filtered unique index is the database safety net:
   one non-cancelled slot per clinician per start time. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Appointments_Staff_Slot' AND object_id = OBJECT_ID(N'dbo.Appointments'))
    CREATE UNIQUE INDEX UX_Appointments_Staff_Slot
        ON dbo.Appointments (StaffId, AppointmentDate, StartTime)
        WHERE [Status] <> 4;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MedicalRecords_PatientId' AND object_id = OBJECT_ID(N'dbo.MedicalRecords'))
    CREATE INDEX IX_MedicalRecords_PatientId ON dbo.MedicalRecords (PatientId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MedicalRecords_CreatedAt' AND object_id = OBJECT_ID(N'dbo.MedicalRecords'))
    CREATE INDEX IX_MedicalRecords_CreatedAt ON dbo.MedicalRecords (CreatedAt);
GO

/* ------------------------------------------------------------------
   5. HELPFUL VIEWS (dashboards / SSMS browsing)
   ------------------------------------------------------------------ */

CREATE OR ALTER VIEW dbo.vw_PatientDirectory
AS
SELECT
    p.Id            AS PatientId,
    p.FirstName,
    p.LastName,
    p.DateOfBirth,
    DATEDIFF(YEAR, p.DateOfBirth, SYSUTCDATETIME())
        - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.DateOfBirth, SYSUTCDATETIME()), p.DateOfBirth) > CONVERT(date, SYSUTCDATETIME()) THEN 1 ELSE 0 END
                    AS Age,
    g.Name          AS Gender,
    p.Email,
    p.Phone,
    p.GhanaCardNumber,
    bg.Name         AS BloodGroup,
    p.City,
    p.Region,
    p.IsActive,
    p.UserId
FROM dbo.Patients p
JOIN dbo.Genders g ON g.GenderId = p.Gender
LEFT JOIN dbo.BloodGroups bg ON bg.BloodGroupId = p.BloodGroup;
GO

CREATE OR ALTER VIEW dbo.vw_AppointmentBoard
AS
SELECT
    a.Id                AS AppointmentId,
    a.AppointmentDate,
    a.StartTime,
    a.EndTime,
    st.Name             AS Status,
    p.FirstName + N' ' + p.LastName AS PatientName,
    p.Phone             AS PatientPhone,
    s.FirstName + N' ' + s.LastName AS StaffName,
    s.Specialization,
    d.Name              AS Department,
    a.Reason
FROM dbo.Appointments a
JOIN dbo.Patients p              ON p.Id = a.PatientId
JOIN dbo.Staff s                 ON s.Id = a.StaffId
JOIN dbo.AppointmentStatuses st  ON st.StatusId = a.Status
LEFT JOIN dbo.Departments d      ON d.Id = s.DepartmentId;
GO

PRINT N'HealthVault schema created.';
GO
