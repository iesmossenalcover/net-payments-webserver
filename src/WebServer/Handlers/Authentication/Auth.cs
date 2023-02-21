using WebServer.Helpers;
using Domain.Entities.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebServer.Handlers.Authentication;

#region Records

public enum SigninStatus
{
    Ok = 1,
    Error = 2,
}

public readonly record struct SigninRequest(string Username, string Password);

public readonly record struct SigninResponse(SigninStatus Status, string? ErrorMessage = null);

public enum SignupStatus
{
    Ok = 1,
    Error = 2,
}

public readonly record struct SignupRequest(string Username, string Password, string FirstName, string LastName);

public readonly record struct SignupResult(SignupStatus Status, string? ErrorMessage = null);

public record IdentityResponse(long UserId, string Username, string GivenName);

#endregion

public class Auth
{
    public static async Task SigninPost(
        [FromBody] SigninRequest model,
        HttpContext ctx,
        Application.Common.Services.IUsersRepository usersRepository,
        IPasswordHasher<User> hasher,
        CancellationToken ct
    )
    {
        User? user = await usersRepository.GetUserByUsernameAsync(model.Username, ct);
        bool correctPassowrd = false;
        if (user != null)
        {
            PasswordVerificationResult pwVerify = hasher.VerifyHashedPassword(user, user.HashedPassword, model.Password);
            if (pwVerify == PasswordVerificationResult.Success)
            {
                correctPassowrd = true;
            }
            else if (pwVerify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                correctPassowrd = true;
                user.HashedPassword = hasher.HashPassword(user, model.Password);
                await usersRepository.UpdateAsync(user, CancellationToken.None);
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

    public static async Task SignupPost(
        [FromBody] SignupRequest model,
        HttpContext ctx,
        Application.Common.Services.IUsersRepository usersRepository,
        IPasswordHasher<User> hasher,
        CancellationToken ct)
    {
        // TODO: validate model, but this is just for text
        User? user = await usersRepository.GetUserByUsernameAsync(model.Username, ct);

        if (user != null)
        {
            var result = new SigninResponse(SigninStatus.Error, "Username already exists");
            await ctx.Response.WriteAsJsonAsync(result, ct);
            return;
        }

        user = new User()
        {
            Username = model.Username,
            Firstname = model.FirstName,
            Lastname = model.LastName,
        };
        user.HashedPassword = hasher.HashPassword(user, model.Password);
        await usersRepository.InsertAsync(user, CancellationToken.None);

        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user.ToClaimPrincipal());
        var res = new SignupResult(SignupStatus.Ok);
        await ctx.Response.WriteAsJsonAsync(res, ct);

    }

    public static async Task GetIdentity(
        HttpContext ctx,
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
