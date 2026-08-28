using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace HealthVault.Web.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly SessionState _session;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(SessionState session)
    {
        _session = session;
        _session.Changed += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_session.IsAuthenticated)
        {
            return Task.FromResult(Anonymous);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(_session.Token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);

        if (!identity.Claims.Any(c => c.Type == ClaimTypes.Role) && !string.IsNullOrWhiteSpace(_session.Role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, _session.Role));
        }

        if (!identity.Claims.Any(c => c.Type == ClaimTypes.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, _session.FullName));
        }

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
