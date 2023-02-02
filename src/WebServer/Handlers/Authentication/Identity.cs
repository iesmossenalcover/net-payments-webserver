using System.Security.Claims;

namespace WebServer.Handlers.Authentication;

#region records

public record IdentityResponse(long userId, string username, string givenName);

#endregion

public class Identity
{
    public static async Task Get(
        HttpContext ctx,
        Application.Common.Services.IAuthenticationService authService,
        Application.Common.Services.ICurrentRequestService currentRequestService,
        CancellationToken ct)
    {
        long? userId = currentRequestService.UserId;

        Claim? usernameClaim = ctx.User.FindFirst(x => x.Type == ClaimTypes.Name);
        string userName = "";
        if (usernameClaim != null)
        {
            userName = usernameClaim.Value;
        }

        Claim? givenNameClaim = ctx.User.FindFirst(x => x.Type == ClaimTypes.GivenName);
        string givenName = "";
        if (givenNameClaim != null)
        {
            givenName = givenNameClaim.Value;
        }

        var respone = new IdentityResponse(userId ?? 0, userName, givenName);
        await ctx.Response.WriteAsJsonAsync(respone, ct);
    }
}
