namespace HealthVault.Domain.Enums;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Staff = "Staff";
    public const string Patient = "Patient";

    public static readonly string[] All = [Administrator, Staff, Patient];
}
