using HealthVault.Application.DTOs.Auth;

namespace HealthVault.Web.Services;

public class SessionState
{
    public AuthResponse? Current { get; private set; }
    public bool IsAuthenticated => Current is not null && !string.IsNullOrWhiteSpace(Current.Token);
    public string Role => Current?.Role ?? string.Empty;
    public string Token => Current?.Token ?? string.Empty;
    public string FullName => Current?.FullName ?? string.Empty;
    public Guid? PatientId => Current?.PatientId;
    public Guid? StaffId => Current?.StaffId;

    public event Action? Changed;

    public void Set(AuthResponse auth)
    {
        Current = auth;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Current = null;
        Changed?.Invoke();
    }
}
