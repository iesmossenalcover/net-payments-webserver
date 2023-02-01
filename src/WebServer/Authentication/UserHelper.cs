using System.Security.Claims;
using Domain.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebServer.Helpers;

public static class UserExtensions
{
    public static ClaimsPrincipal ToClaimPrincipal(this User u)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Name, u.Username),
            new Claim(ClaimTypes.GivenName, $"{u.Firstname} {u.Lastname}"),
        };

        if (u.UserClaims != null)
            claims.AddRange(u.UserClaims.Select(x => new Claim(x.Type, x.Value)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}