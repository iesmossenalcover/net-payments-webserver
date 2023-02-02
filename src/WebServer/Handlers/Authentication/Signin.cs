using WebServer.Helpers;
using Domain.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers.Authentication;

#region Records

public enum SigninStatus
{
    Ok = 1,
    Error = 2,
}

public readonly record struct SigninRequest(string username, string password);

public readonly record struct SigninResponse(SigninStatus status, string? errorMessage = null);

#endregion

public class Signin
{
    public static async Task Post(
        [FromBody] SigninRequest model,
        HttpContext ctx,
        Application.Common.Services.IAuthenticationService authService,
        IPasswordHasher<User> hasher,
        CancellationToken ct
    )
    {
        User? user = await authService.GetUserAsync(model.username, ct);
        bool correctPassowrd = false;
        if (user != null)
        {
            PasswordVerificationResult pwVerify = hasher.VerifyHashedPassword(user, user.HashedPassword, model.password);
            if (pwVerify == PasswordVerificationResult.Success)
            {
                correctPassowrd = true;
            }
            else if (pwVerify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                correctPassowrd = true;
                user.HashedPassword = hasher.HashPassword(user, model.password);
                await authService.UpdateUserAsync(user, CancellationToken.None);
            }
        }

        if (user == null || !correctPassowrd)
        {
            var result = new SigninResponse(SigninStatus.Error, "Invalid user or password");
            await ctx.Response.WriteAsJsonAsync(result, ct);
            return;
        }

        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user.ToClaimPrincipal());
        var res = new SigninResponse(SigninStatus.Ok);
        await ctx.Response.WriteAsJsonAsync(res, ct);

    }
}
