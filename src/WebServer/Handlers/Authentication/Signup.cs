using WebServer.Helpers;
using Domain.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Handlers.Authentication;

#region Records

public enum SignupStatus
{
    Ok = 1,
    Error = 2,
}

public readonly record struct SignupRequest(string username, string password, string firstName, string lastName);

public readonly record struct SignupResult(SignupStatus status, string? errorMessage = null);

#endregion

public class Signup
{
    public static async Task Post(
        [FromBody] SignupRequest model,
        HttpContext ctx,
        Application.Common.Services.IAuthenticationService authService,
        IPasswordHasher<User> hasher,
        CancellationToken ct)
    {
        // TODO: validate model, but this is just for text
        User? user = await authService.GetUserAsync(model.username, ct);

        if (user != null)
        {
            var result = new SigninResponse(SigninStatus.Error, "Username already exists");
            await ctx.Response.WriteAsJsonAsync(result, ct);
            return;
        }

        user = new User()
        {
            Username = model.username,
            Firstname = model.firstName,
            Lastname = model.lastName,
        };
        user.HashedPassword = hasher.HashPassword(user, model.password);
        user.Id = await authService.InsertUserAsync(user, CancellationToken.None);

        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user.ToClaimPrincipal());
        var res = new SignupResult(SignupStatus.Ok);
        await ctx.Response.WriteAsJsonAsync(res, ct);

    }
}
