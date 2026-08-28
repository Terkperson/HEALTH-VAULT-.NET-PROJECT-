from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from openpyxl.utils import get_column_letter
from openpyxl.worksheet.datavalidation import DataValidation
from openpyxl.chart import BarChart, Reference
from openpyxl.worksheet.table import Table, TableStyleInfo
from pathlib import Path

wb = Workbook()

teal = PatternFill("solid", fgColor="0D6E6E")
teal_l = PatternFill("solid", fgColor="E7F2F0")
navy = PatternFill("solid", fgColor="14324F")
gold = PatternFill("solid", fgColor="F6E7B2")
white = Font(color="FFFFFF", bold=True, name="Calibri")
title_font = Font(name="Calibri", size=18, bold=True, color="0D6E6E")
head_font = Font(name="Calibri", size=11, bold=True, color="FFFFFF")
body = Font(name="Calibri", size=11)
thin = Border(
    left=Side(style="thin", color="D5E0DC"),
    right=Side(style="thin", color="D5E0DC"),
    top=Side(style="thin", color="D5E0DC"),
    bottom=Side(style="thin", color="D5E0DC"),
)
wrap = Alignment(wrap_text=True, vertical="center")


def style_header(ws, row, cols):
    for c in range(1, cols + 1):
        cell = ws.cell(row, c)
        cell.fill = teal
        cell.font = head_font
        cell.alignment = Alignment(vertical="center", wrap_text=True)
        cell.border = thin


def autosize(ws, widths):
    for i, w in enumerate(widths, 1):
        ws.column_dimensions[get_column_letter(i)].width = w


def paint_body(ws, start, end, cols):
    for r in range(start, end + 1):
        for c in range(1, cols + 1):
            cell = ws.cell(r, c)
            cell.font = body
            cell.alignment = wrap
            cell.border = thin
            if r % 2 == 0:
                if cell.fill.fgColor is None or cell.fill.fgColor.rgb == "00000000":
                    cell.fill = PatternFill("solid", fgColor="F7FBFA")


# ---------------------------------------------------------------------------
# Cover
# ---------------------------------------------------------------------------
cover = wb.active
cover.title = "Overview"
cover["A1"] = "HealthVault — Relational Database Catalog"
cover["A1"].font = title_font
cover.merge_cells("A1:F1")
cover["A2"] = "SRS v1.1 · SQL Server 2019+ / Azure SQL · EF Core 8 · August 2026"
cover["A2"].font = Font(name="Calibri", italic=True, color="5B6B76")

facts = [
    ("Database", "HealthVaultDb"),
    ("Engine", "Microsoft SQL Server"),
    ("Tables", "18 (7 Identity + 5 lookup + 4 domain + 2 unused Identity support)"),
    ("Views", "vw_PatientDirectory, vw_AppointmentBoard"),
    ("Scripts", "database/01_CreateDatabase.sql → 02_CreateTables.sql → 03_SeedData.sql"),
    ("Role helper", "database/04_AssignRole.sql  (FR-20, no UI)"),
    ("Demo logins", "Created by the API seeder, not by T-SQL (password hashes)"),
]
cover["A4"] = "Item"
cover["B4"] = "Value"
style_header(cover, 4, 2)
for i, (k, v) in enumerate(facts, 5):
    cover.cell(i, 1, k)
    cover.cell(i, 2, v)
paint_body(cover, 5, 11, 2)
autosize(cover, [22, 88])
cover.row_dimensions[1].height = 28

cover["A13"] = "How to run in SSMS / Azure Data Studio / any SQL workbench"
cover["A13"].font = Font(name="Calibri", size=14, bold=True, color="094F4F")
steps = [
    "1. Connect to your SQL Server instance (Windows auth or SQL login).",
    "2. Execute database/01_CreateDatabase.sql  — creates HealthVaultDb.",
    "3. Execute database/02_CreateTables.sql   — tables, FKs, indexes, views.",
    "4. Execute database/03_SeedData.sql       — lookups + three Identity roles.",
    "5. Set ConnectionStrings:DefaultConnection in src/HealthVault.Api/appsettings.json.",
    "6. Start the API once so Identity can seed hashed demo users.",
    "7. Browse tables in Object Explorer. Useful queries are in database/05_UsefulQueries.sql.",
]
for i, s in enumerate(steps, 14):
    cover.cell(i, 1, s)
    cover.merge_cells(start_row=i, start_column=1, end_row=i, end_column=6)

# ---------------------------------------------------------------------------
# Table inventory
# ---------------------------------------------------------------------------
inv = wb.create_sheet("Table inventory")
inv["A1"] = "All tables in HealthVaultDb"
inv["A1"].font = title_font
inv.merge_cells("A1:F1")
headers = ["#", "Table", "Group", "Primary key", "Rows (typical)", "Purpose"]
for i, h in enumerate(headers, 1):
    inv.cell(3, i, h)
style_header(inv, 3, 6)

tables = [
    (1, "Genders", "Lookup", "GenderId", "4", "Sex / gender codes used by Patients"),
    (2, "BloodGroups", "Lookup", "BloodGroupId", "9", "ABO/Rh codes"),
    (3, "AppointmentStatuses", "Lookup", "StatusId", "4", "Pending, Confirmed, Completed, Cancelled"),
    (4, "Departments", "Lookup", "Id IDENTITY", "5", "Clinic departments for Staff"),
    (5, "MedicalRecordCategories", "Lookup", "Id IDENTITY", "5", "MVP default = General; extras ready for v1.1"),
    (6, "AspNetUsers", "Identity", "Id", "grows", "Logins. Extended with FirstName, LastName, IsActive, timestamps"),
    (7, "AspNetRoles", "Identity", "Id", "3", "Administrator, Staff, Patient"),
    (8, "AspNetUserRoles", "Identity", "(UserId, RoleId)", "grows", "Which role a user currently has"),
    (9, "AspNetUserClaims", "Identity", "Id IDENTITY", "0", "Required by Identity; unused in MVP UI"),
    (10, "AspNetRoleClaims", "Identity", "Id IDENTITY", "0", "Required by Identity; unused in MVP UI"),
    (11, "AspNetUserLogins", "Identity", "(LoginProvider, ProviderKey)", "0", "External logins; unused"),
    (12, "AspNetUserTokens", "Identity", "(UserId, LoginProvider, Name)", "0", "Identity tokens; unused"),
    (13, "Staff", "Domain", "Id", "grows", "Clinician / nurse / admin profile, 1:1 with a user"),
    (14, "Patients", "Domain", "Id", "grows", "Clinical patient record; UserId optional"),
    (15, "Appointments", "Domain", "Id", "grows", "Booked slots with status workflow"),
    (16, "MedicalRecords", "Domain", "Id", "grows", "Uploaded files. No delete in MVP"),
]
for r, row in enumerate(tables, 4):
    for c, val in enumerate(row, 1):
        inv.cell(r, c, val)
paint_body(inv, 4, 19, 6)
autosize(inv, [6, 28, 12, 32, 16, 62])
inv.auto_filter.ref = "A3:F19"
inv.freeze_panes = "A4"

# ---------------------------------------------------------------------------
# Columns
# ---------------------------------------------------------------------------
cols = wb.create_sheet("Columns")
cols["A1"] = "Column-level catalog (every user table)"
cols["A1"].font = title_font
ch = ["Table", "Column", "Data type", "Null", "Key", "Default / check", "Description"]
for i, h in enumerate(ch, 1):
    cols.cell(3, i, h)
style_header(cols, 3, 7)

columns = [
    # Genders
    ("Genders", "GenderId", "TINYINT", "NO", "PK", "", "1 Female, 2 Male, 3 Other, 4 Prefer not to say"),
    ("Genders", "Code", "NVARCHAR(20)", "NO", "UQ", "", "Stable machine code"),
    ("Genders", "Name", "NVARCHAR(50)", "NO", "", "", "Display label"),
    # Blood
    ("BloodGroups", "BloodGroupId", "TINYINT", "NO", "PK", "", "1–8 ABO/Rh, 9 Unknown"),
    ("BloodGroups", "Code", "NVARCHAR(8)", "NO", "UQ", "", "A+, O−, UNK…"),
    ("BloodGroups", "Name", "NVARCHAR(20)", "NO", "", "", "Display label"),
    # Status
    ("AppointmentStatuses", "StatusId", "TINYINT", "NO", "PK", "", "Matches C# enum 1–4"),
    ("AppointmentStatuses", "Code", "NVARCHAR(20)", "NO", "UQ", "", ""),
    ("AppointmentStatuses", "Name", "NVARCHAR(40)", "NO", "", "", ""),
    ("AppointmentStatuses", "SortOrder", "TINYINT", "NO", "", "", "UI order"),
    # Dept
    ("Departments", "Id", "INT IDENTITY", "NO", "PK", "", ""),
    ("Departments", "Name", "NVARCHAR(120)", "NO", "UQ", "", "GP, Outpatient, Lab, Pharmacy, Admin"),
    ("Departments", "Description", "NVARCHAR(400)", "YES", "", "", ""),
    ("Departments", "IsActive", "BIT", "NO", "", "1", ""),
    # Categories
    ("MedicalRecordCategories", "Id", "INT IDENTITY", "NO", "PK", "", ""),
    ("MedicalRecordCategories", "Code", "NVARCHAR(40)", "NO", "UQ", "", "GENERAL is the MVP default"),
    ("MedicalRecordCategories", "Name", "NVARCHAR(80)", "NO", "", "", ""),
    ("MedicalRecordCategories", "IsDefault", "BIT", "NO", "", "0", "Exactly one row should be 1"),
    ("MedicalRecordCategories", "IsActive", "BIT", "NO", "", "1", ""),
    # Users
    ("AspNetUsers", "Id", "NVARCHAR(450)", "NO", "PK", "", "Identity string key"),
    ("AspNetUsers", "UserName", "NVARCHAR(256)", "YES", "", "", ""),
    ("AspNetUsers", "NormalizedUserName", "NVARCHAR(256)", "YES", "UQ*", "", "Filtered unique index"),
    ("AspNetUsers", "Email", "NVARCHAR(256)", "YES", "", "", "Required unique by the app"),
    ("AspNetUsers", "NormalizedEmail", "NVARCHAR(256)", "YES", "IX", "", ""),
    ("AspNetUsers", "EmailConfirmed", "BIT", "NO", "", "0", ""),
    ("AspNetUsers", "PasswordHash", "NVARCHAR(MAX)", "YES", "", "", "Never store plain text"),
    ("AspNetUsers", "SecurityStamp", "NVARCHAR(MAX)", "YES", "", "", "Identity internal"),
    ("AspNetUsers", "ConcurrencyStamp", "NVARCHAR(MAX)", "YES", "", "", "Identity internal"),
    ("AspNetUsers", "PhoneNumber", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetUsers", "PhoneNumberConfirmed", "BIT", "NO", "", "0", ""),
    ("AspNetUsers", "TwoFactorEnabled", "BIT", "NO", "", "0", ""),
    ("AspNetUsers", "LockoutEnd", "DATETIMEOFFSET", "YES", "", "", ""),
    ("AspNetUsers", "LockoutEnabled", "BIT", "NO", "", "1", ""),
    ("AspNetUsers", "AccessFailedCount", "INT", "NO", "", "0", ""),
    ("AspNetUsers", "FirstName", "NVARCHAR(80)", "NO", "", "", "HealthVault extension"),
    ("AspNetUsers", "LastName", "NVARCHAR(80)", "NO", "", "", "HealthVault extension"),
    ("AspNetUsers", "IsActive", "BIT", "NO", "", "1", "Soft-disable login"),
    ("AspNetUsers", "CreatedAt", "DATETIME2(7)", "NO", "", "SYSUTCDATETIME()", "UTC"),
    ("AspNetUsers", "UpdatedAt", "DATETIME2(7)", "YES", "", "", "UTC"),
    # Roles
    ("AspNetRoles", "Id", "NVARCHAR(450)", "NO", "PK", "", ""),
    ("AspNetRoles", "Name", "NVARCHAR(256)", "YES", "", "", "Administrator / Staff / Patient"),
    ("AspNetRoles", "NormalizedName", "NVARCHAR(256)", "YES", "UQ*", "", "Filtered unique"),
    ("AspNetRoles", "ConcurrencyStamp", "NVARCHAR(MAX)", "YES", "", "", ""),
    # UserRoles
    ("AspNetUserRoles", "UserId", "NVARCHAR(450)", "NO", "PK, FK", "", "→ AspNetUsers.Id"),
    ("AspNetUserRoles", "RoleId", "NVARCHAR(450)", "NO", "PK, FK", "", "→ AspNetRoles.Id"),
    # Identity misc
    ("AspNetUserClaims", "Id", "INT IDENTITY", "NO", "PK", "", ""),
    ("AspNetUserClaims", "UserId", "NVARCHAR(450)", "NO", "FK", "", "→ AspNetUsers.Id CASCADE"),
    ("AspNetUserClaims", "ClaimType", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetUserClaims", "ClaimValue", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetRoleClaims", "Id", "INT IDENTITY", "NO", "PK", "", ""),
    ("AspNetRoleClaims", "RoleId", "NVARCHAR(450)", "NO", "FK", "", "→ AspNetRoles.Id CASCADE"),
    ("AspNetRoleClaims", "ClaimType", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetRoleClaims", "ClaimValue", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetUserLogins", "LoginProvider", "NVARCHAR(128)", "NO", "PK", "", ""),
    ("AspNetUserLogins", "ProviderKey", "NVARCHAR(128)", "NO", "PK", "", ""),
    ("AspNetUserLogins", "ProviderDisplayName", "NVARCHAR(MAX)", "YES", "", "", ""),
    ("AspNetUserLogins", "UserId", "NVARCHAR(450)", "NO", "FK", "", "→ AspNetUsers.Id CASCADE"),
    ("AspNetUserTokens", "UserId", "NVARCHAR(450)", "NO", "PK, FK", "", "→ AspNetUsers.Id CASCADE"),
    ("AspNetUserTokens", "LoginProvider", "NVARCHAR(128)", "NO", "PK", "", ""),
    ("AspNetUserTokens", "Name", "NVARCHAR(128)", "NO", "PK", "", ""),
    ("AspNetUserTokens", "Value", "NVARCHAR(MAX)", "YES", "", "", ""),
    # Staff
    ("Staff", "Id", "UNIQUEIDENTIFIER", "NO", "PK", "NEWSEQUENTIALID()", ""),
    ("Staff", "UserId", "NVARCHAR(450)", "NO", "FK, UQ", "", "→ AspNetUsers.Id  (1:1)"),
    ("Staff", "DepartmentId", "INT", "YES", "FK", "", "→ Departments.Id SET NULL"),
    ("Staff", "FirstName", "NVARCHAR(80)", "NO", "", "", ""),
    ("Staff", "LastName", "NVARCHAR(80)", "NO", "IX", "", "Composite with FirstName"),
    ("Staff", "EmployeeNumber", "NVARCHAR(30)", "YES", "UQ*", "", "Filtered unique when present"),
    ("Staff", "Specialization", "NVARCHAR(120)", "YES", "", "", "e.g. General Practitioner"),
    ("Staff", "Phone", "NVARCHAR(30)", "YES", "", "", ""),
    ("Staff", "HireDate", "DATE", "YES", "", "", ""),
    ("Staff", "IsActive", "BIT", "NO", "", "1", ""),
    ("Staff", "CreatedAt", "DATETIME2(7)", "NO", "", "SYSUTCDATETIME()", "UTC"),
    ("Staff", "UpdatedAt", "DATETIME2(7)", "YES", "", "", "UTC"),
    # Patients
    ("Patients", "Id", "UNIQUEIDENTIFIER", "NO", "PK", "NEWSEQUENTIALID()", ""),
    ("Patients", "UserId", "NVARCHAR(450)", "YES", "FK, UQ*", "", "NULL if staff-created without a portal login"),
    ("Patients", "FirstName", "NVARCHAR(80)", "NO", "IX", "", "Search"),
    ("Patients", "LastName", "NVARCHAR(80)", "NO", "IX", "", "Search"),
    ("Patients", "DateOfBirth", "DATE", "NO", "", "CHECK ≤ today", ""),
    ("Patients", "Gender", "TINYINT", "NO", "FK", "", "→ Genders.GenderId"),
    ("Patients", "Email", "NVARCHAR(256)", "YES", "IX", "", "Search"),
    ("Patients", "Phone", "NVARCHAR(30)", "NO", "IX", "", "Search"),
    ("Patients", "GhanaCardNumber", "NVARCHAR(20)", "YES", "IX", "", "National ID, optional"),
    ("Patients", "BloodGroup", "TINYINT", "YES", "FK", "", "→ BloodGroups.BloodGroupId"),
    ("Patients", "AddressLine1", "NVARCHAR(200)", "YES", "", "", ""),
    ("Patients", "AddressLine2", "NVARCHAR(200)", "YES", "", "", ""),
    ("Patients", "City", "NVARCHAR(80)", "YES", "", "", ""),
    ("Patients", "Region", "NVARCHAR(80)", "YES", "", "", "e.g. Greater Accra"),
    ("Patients", "PostalCode", "NVARCHAR(20)", "YES", "", "", ""),
    ("Patients", "Country", "NVARCHAR(80)", "NO", "N'Ghana'", "", ""),
    ("Patients", "EmergencyContactName", "NVARCHAR(160)", "YES", "", "", ""),
    ("Patients", "EmergencyContactPhone", "NVARCHAR(30)", "YES", "", "", ""),
    ("Patients", "EmergencyContactRelation", "NVARCHAR(50)", "YES", "", "", ""),
    ("Patients", "Allergies", "NVARCHAR(500)", "YES", "", "", ""),
    ("Patients", "Notes", "NVARCHAR(1000)", "YES", "", "", ""),
    ("Patients", "IsActive", "BIT", "NO", "", "1", ""),
    ("Patients", "CreatedAt", "DATETIME2(7)", "NO", "", "SYSUTCDATETIME()", "UTC"),
    ("Patients", "UpdatedAt", "DATETIME2(7)", "YES", "", "", "UTC"),
    ("Patients", "CreatedByUserId", "NVARCHAR(450)", "YES", "", "", "Who registered the patient"),
    # Appointments
    ("Appointments", "Id", "UNIQUEIDENTIFIER", "NO", "PK", "NEWSEQUENTIALID()", ""),
    ("Appointments", "PatientId", "UNIQUEIDENTIFIER", "NO", "FK, IX", "", "→ Patients.Id RESTRICT"),
    ("Appointments", "StaffId", "UNIQUEIDENTIFIER", "NO", "FK, IX", "", "→ Staff.Id RESTRICT"),
    ("Appointments", "AppointmentDate", "DATE", "NO", "IX", "", ""),
    ("Appointments", "StartTime", "TIME(0)", "NO", "", "", "Clinic hours enforced in app 08:00–18:00"),
    ("Appointments", "EndTime", "TIME(0)", "NO", "", "CHECK > StartTime", "Default duration 30 minutes"),
    ("Appointments", "Status", "TINYINT", "NO", "FK, IX", "1", "→ AppointmentStatuses"),
    ("Appointments", "Reason", "NVARCHAR(300)", "NO", "", "", ""),
    ("Appointments", "Notes", "NVARCHAR(1000)", "YES", "", "", ""),
    ("Appointments", "CancellationReason", "NVARCHAR(400)", "YES", "", "", ""),
    ("Appointments", "CancelledAt", "DATETIME2(7)", "YES", "", "", ""),
    ("Appointments", "CancelledByUserId", "NVARCHAR(450)", "YES", "", "", ""),
    ("Appointments", "CreatedAt", "DATETIME2(7)", "NO", "", "SYSUTCDATETIME()", ""),
    ("Appointments", "UpdatedAt", "DATETIME2(7)", "YES", "", "", ""),
    ("Appointments", "CreatedByUserId", "NVARCHAR(450)", "YES", "", "", ""),
    # Records
    ("MedicalRecords", "Id", "UNIQUEIDENTIFIER", "NO", "PK", "NEWSEQUENTIALID()", ""),
    ("MedicalRecords", "PatientId", "UNIQUEIDENTIFIER", "NO", "FK, IX", "", "→ Patients.Id RESTRICT"),
    ("MedicalRecords", "UploadedByUserId", "NVARCHAR(450)", "NO", "FK", "", "→ AspNetUsers.Id RESTRICT"),
    ("MedicalRecords", "CategoryId", "INT", "NO", "FK", "", "MVP always the default General row"),
    ("MedicalRecords", "Title", "NVARCHAR(200)", "NO", "", "", ""),
    ("MedicalRecords", "Description", "NVARCHAR(1000)", "YES", "", "", ""),
    ("MedicalRecords", "OriginalFileName", "NVARCHAR(260)", "NO", "", "", "Name shown to the user"),
    ("MedicalRecords", "StoredFileName", "NVARCHAR(260)", "NO", "", "", "GUID + extension on disk"),
    ("MedicalRecords", "FilePath", "NVARCHAR(500)", "NO", "", "", "Relative path under App_Data/uploads"),
    ("MedicalRecords", "ContentType", "NVARCHAR(120)", "NO", "", "", "PDF / JPG / PNG / DOCX"),
    ("MedicalRecords", "FileSizeBytes", "BIGINT", "NO", "", "CHECK 1..10485760", "10 MB cap"),
    ("MedicalRecords", "CreatedAt", "DATETIME2(7)", "NO", "IX", "SYSUTCDATETIME()", "Upload timestamp"),
    ("MedicalRecords", "UpdatedAt", "DATETIME2(7)", "YES", "", "", ""),
]

for r, row in enumerate(columns, 4):
    for c, val in enumerate(row, 1):
        cell = cols.cell(r, c, val)
        if val in ("PK", "PK, FK"):
            cell.fill = PatternFill("solid", fgColor="C6E6E2")
        elif "FK" in str(val):
            cell.fill = PatternFill("solid", fgColor="FDE7CD")
paint_body(cols, 4, 3 + len(columns), 7)
autosize(cols, [26, 26, 22, 8, 12, 28, 56])
cols.auto_filter.ref = f"A3:G{3 + len(columns)}"
cols.freeze_panes = "A4"

# ---------------------------------------------------------------------------
# Relationships
# ---------------------------------------------------------------------------
rel = wb.create_sheet("Relationships")
rel["A1"] = "Foreign keys and cardinality"
rel["A1"].font = title_font
rh = ["Parent table", "Parent key", "Child table", "Child column", "Cardinality", "On delete"]
for i, h in enumerate(rh, 1):
    rel.cell(3, i, h)
style_header(rel, 3, 6)
rels = [
    ("AspNetUsers", "Id", "AspNetUserRoles", "UserId", "1 : N", "CASCADE"),
    ("AspNetRoles", "Id", "AspNetUserRoles", "RoleId", "1 : N", "CASCADE"),
    ("AspNetUsers", "Id", "AspNetUserClaims", "UserId", "1 : N", "CASCADE"),
    ("AspNetRoles", "Id", "AspNetRoleClaims", "RoleId", "1 : N", "CASCADE"),
    ("AspNetUsers", "Id", "AspNetUserLogins", "UserId", "1 : N", "CASCADE"),
    ("AspNetUsers", "Id", "AspNetUserTokens", "UserId", "1 : N", "CASCADE"),
    ("AspNetUsers", "Id", "Patients", "UserId", "1 : 0..1", "SET NULL"),
    ("AspNetUsers", "Id", "Staff", "UserId", "1 : 0..1", "RESTRICT / NO ACTION"),
    ("Departments", "Id", "Staff", "DepartmentId", "1 : N", "SET NULL"),
    ("Genders", "GenderId", "Patients", "Gender", "1 : N", "RESTRICT / NO ACTION"),
    ("BloodGroups", "BloodGroupId", "Patients", "BloodGroup", "1 : N", "RESTRICT / NO ACTION"),
    ("Patients", "Id", "Appointments", "PatientId", "1 : N", "RESTRICT / NO ACTION"),
    ("Staff", "Id", "Appointments", "StaffId", "1 : N", "RESTRICT / NO ACTION"),
    ("AppointmentStatuses", "StatusId", "Appointments", "Status", "1 : N", "RESTRICT / NO ACTION"),
    ("Patients", "Id", "MedicalRecords", "PatientId", "1 : N", "RESTRICT / NO ACTION"),
    ("AspNetUsers", "Id", "MedicalRecords", "UploadedByUserId", "1 : N", "RESTRICT / NO ACTION"),
    ("MedicalRecordCategories", "Id", "MedicalRecords", "CategoryId", "1 : N", "RESTRICT / NO ACTION"),
]
for r, row in enumerate(rels, 4):
    for c, val in enumerate(row, 1):
        rel.cell(r, c, val)
paint_body(rel, 4, 3 + len(rels), 6)
autosize(rel, [26, 14, 26, 22, 14, 24])
rel.freeze_panes = "A4"

# ---------------------------------------------------------------------------
# Indexes
# ---------------------------------------------------------------------------
idx = wb.create_sheet("Indexes")
idx["A1"] = "Indexes (including the double-booking safety net)"
idx["A1"].font = title_font
ih = ["Table", "Index", "Columns", "Unique", "Filter"]
for i, h in enumerate(ih, 1):
    idx.cell(3, i, h)
style_header(idx, 3, 5)
indexes = [
    ("AspNetRoles", "RoleNameIndex", "NormalizedName", "YES", "NormalizedName IS NOT NULL"),
    ("AspNetUsers", "UserNameIndex", "NormalizedUserName", "YES", "NormalizedUserName IS NOT NULL"),
    ("AspNetUsers", "EmailIndex", "NormalizedEmail", "NO", ""),
    ("Staff", "UQ_Staff_UserId", "UserId", "YES", ""),
    ("Staff", "UQ_Staff_EmployeeNumber", "EmployeeNumber", "YES", "EmployeeNumber IS NOT NULL"),
    ("Staff", "IX_Staff_Name", "LastName, FirstName", "NO", ""),
    ("Patients", "UQ_Patients_UserId", "UserId", "YES", "UserId IS NOT NULL"),
    ("Patients", "IX_Patients_Email", "Email", "NO", ""),
    ("Patients", "IX_Patients_Phone", "Phone", "NO", ""),
    ("Patients", "IX_Patients_GhanaCard", "GhanaCardNumber", "NO", ""),
    ("Patients", "IX_Patients_Name", "LastName, FirstName", "NO", ""),
    ("Appointments", "IX_Appointments_PatientId", "PatientId", "NO", ""),
    ("Appointments", "IX_Appointments_StaffId", "StaffId", "NO", ""),
    ("Appointments", "IX_Appointments_Date", "AppointmentDate", "NO", ""),
    ("Appointments", "IX_Appointments_Status", "Status", "NO", ""),
    ("Appointments", "UX_Appointments_Staff_Slot", "StaffId, AppointmentDate, StartTime", "YES", "Status <> 4  (not cancelled)"),
    ("MedicalRecords", "IX_MedicalRecords_PatientId", "PatientId", "NO", ""),
    ("MedicalRecords", "IX_MedicalRecords_CreatedAt", "CreatedAt", "NO", ""),
]
for r, row in enumerate(indexes, 4):
    for c, val in enumerate(row, 1):
        cell = idx.cell(r, c, val)
        if row[3] == "YES":
            cell.fill = PatternFill("solid", fgColor="E8F4F2")
paint_body(idx, 4, 3 + len(indexes), 5)
autosize(idx, [22, 32, 40, 10, 36])

# ---------------------------------------------------------------------------
# Seed
# ---------------------------------------------------------------------------
seed = wb.create_sheet("Seed data")
seed["A1"] = "Rows inserted by 03_SeedData.sql and by the API seeder"
seed["A1"].font = title_font
seed["A3"] = "Lookups (T-SQL)"
seed["A3"].font = Font(bold=True, color="094F4F")
seed["A4"] = "Genders 1–4 · BloodGroups 1–9 · AppointmentStatuses 1–4 · 5 Departments · 5 record categories (General is default)"
seed["A6"] = "Identity roles (T-SQL and/or API)"
seed["A6"].font = Font(bold=True, color="094F4F")
seed["A7"] = "Administrator · Staff · Patient"
seed["A9"] = "Demo users — created by HealthVault.Api on first start (hashed passwords)"
seed["A9"].font = Font(bold=True, color="094F4F")
sh = ["Email", "Password", "Role", "Notes"]
for i, h in enumerate(sh, 1):
    seed.cell(10, i, h)
style_header(seed, 10, 4)
users = [
    ("admin@healthvault.com", "Admin@12345", "Administrator", "Ama Mensah · HV-ADM-001 · Administration"),
    ("staff@healthvault.com", "Staff@12345", "Staff", "Kwame Boateng · GP · General Practice"),
    ("nurse@healthvault.com", "Staff@12345", "Staff", "Efua Owusu · RN · Outpatient"),
    ("patient@healthvault.com", "Patient@12345", "Patient", "Kojo Asante · Medina Estates · O+ · Ghana Card sample"),
]
for r, row in enumerate(users, 11):
    for c, val in enumerate(row, 1):
        seed.cell(r, c, val)
paint_body(seed, 11, 14, 4)
autosize(seed, [32, 18, 16, 62])

seed["A16"] = "To change a user's role in the MVP window, edit and run database/04_AssignRole.sql — there is no role UI (SRS FR-20)."
seed["A16"].fill = gold

# ---------------------------------------------------------------------------
# Connection
# ---------------------------------------------------------------------------
conn = wb.create_sheet("Connection strings")
conn["A1"] = "Paste one of these into src/HealthVault.Api/appsettings.json"
conn["A1"].font = title_font
conn["A3"] = "Windows authentication (SSMS on the same PC)"
conn["A4"] = "Server=localhost;Database=HealthVaultDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
conn["A6"] = "SQL Server login (lab / named instance)"
conn["A7"] = "Server=localhost;Database=HealthVaultDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=True"
conn["A9"] = "Named instance example"
conn["A10"] = "Server=.\\SQLEXPRESS;Database=HealthVaultDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
for addr in ("A4", "A7", "A10"):
    conn[addr].font = Font(name="Consolas", size=11)
    conn[addr].fill = teal_l
autosize(conn, [140])

out = Path("/home/user/HealthVault/docs/HealthVault_Tables.xlsx")
wb.save(out)
print(out, "sheets:", wb.sheetnames)
