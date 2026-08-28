/*
    HealthVault — MVP role assignment helper
    SRS FR-20: admins view users/roles in the UI.
    Changing a role in the 2-week window is a direct database update.

    Usage (edit the two variables, then run the whole script):
*/
USE HealthVaultDb;
GO

DECLARE @Email    NVARCHAR(256) = N'patient@healthvault.com';  -- target login
DECLARE @NewRole  NVARCHAR(256) = N'Staff';                    -- Administrator | Staff | Patient

DECLARE @UserId NVARCHAR(450) =
    (SELECT Id FROM dbo.AspNetUsers WHERE NormalizedEmail = UPPER(@Email));

DECLARE @RoleId NVARCHAR(450) =
    (SELECT Id FROM dbo.AspNetRoles WHERE NormalizedName = UPPER(@NewRole));

IF @UserId IS NULL
BEGIN
    RAISERROR(N'No user found for email %s', 16, 1, @Email);
    RETURN;
END

IF @RoleId IS NULL
BEGIN
    RAISERROR(N'Unknown role %s. Use Administrator, Staff or Patient.', 16, 1, @NewRole);
    RETURN;
END

DELETE FROM dbo.AspNetUserRoles WHERE UserId = @UserId;
INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);

PRINT N'Assigned ' + @NewRole + N' to ' + @Email + N'.';

/* If promoting to Staff, create a Staff profile when one does not exist. */
IF UPPER(@NewRole) = N'STAFF'
   AND NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE UserId = @UserId)
BEGIN
    INSERT INTO dbo.Staff (UserId, FirstName, LastName, EmployeeNumber, Specialization, Phone, IsActive, CreatedAt)
    SELECT Id, FirstName, LastName, NULL, N'General Practitioner', PhoneNumber, 1, SYSUTCDATETIME()
    FROM dbo.AspNetUsers
    WHERE Id = @UserId;
    PRINT N'Staff profile created.';
END
GO
