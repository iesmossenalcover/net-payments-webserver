using WebServer.Helpers;
using Domain.Entities.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Common.Services;

namespace WebServer.Handlers.Authentication;

#region Records

public enum SigninStatus
{
    Ok = 1,
    Error = 2,
    Unauthorized = 3,
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

public record OAuthSignIn(string Token);

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


    // public class GoogleAuthResponse
    // {
    //     [JsonPropertyName("iss")]
    //     public string Iss { get; set; } = string.Empty;

    //     [JsonPropertyName("nbf")]
    //     public string Nbf { get; set; } = string.Empty;

    //     [JsonPropertyName("aud")]
    //     public string Aud { get; set; } = string.Empty;

    //     [JsonPropertyName("sub")]
    //     public string Sub { get; set; } = string.Empty;

    //     [JsonPropertyName("email")]
    //     public string Email { get; set; } = string.Empty;

    //     [JsonPropertyName("email_verified")]
    //     public string EmailVerified { get; set; } = string.Empty;

    //     [JsonPropertyName("azp")]
    //     public string Azp { get; set; } = string.Empty;

    //     [JsonPropertyName("name")]
    //     public string Name { get; set; } = string.Empty;

    //     [JsonPropertyName("picture")]
    //     public string Picture { get; set; } = string.Empty;

    //     [JsonPropertyName("given_name")]
    //     public string GivenName { get; set; } = string.Empty;

    //     [JsonPropertyName("family_name")]
    //     public string FamilyName { get; set; } = string.Empty;

    //     [JsonPropertyName("iat")]
    //     public string Iat { get; set; } = string.Empty;

    //     [JsonPropertyName("exp")]
    //     public string Exp { get; set; } = string.Empty;

    //     [JsonPropertyName("jti")]
    //     public string Jti { get; set; } = string.Empty;

    //     [JsonPropertyName("alg")]
    //     public string Alg { get; set; } = string.Empty;

    //     [JsonPropertyName("kid")]
    //     public string Kid { get; set; } = string.Empty;

    //     [JsonPropertyName("typ")]
    //     public string Typ { get; set; } = string.Empty;
    // }

    public static async Task SigninOAuth(
        HttpContext ctx,
        [FromBody] OAuthSignIn model,
        IOAuthRepository oAuthRepository,
        IOAuthUsersRepository oAuthUsersRepository,
        IGoogleAdminApi adminApi,
        CancellationToken ct)
    {
        IDictionary<string, string>? fields = await oAuthRepository.TokenInfoValidation(model.Token, ct);
        if (fields == null || !fields.Any())
        {
            var result = new SigninResponse(SigninStatus.Error, "Invalid token");
            await ctx.Response.WriteAsJsonAsync(result, ct);
            return;
        }

        string? subject = string.Empty;
        string? email = string.Empty;
        if (!fields.TryGetValue("sub", out subject) ||
            !fields.TryGetValue("email", out email))
        {
            var result = new SigninResponse(SigninStatus.Error, "Invalid token");
            await ctx.Response.WriteAsJsonAsync(result, ct);
            return;
        }

        // Try to get user from db.
        OAuthUser? oAuthUser = await oAuthUsersRepository.GetUserBySubjectAndCodeAsync(subject, "google", ct);
        User? user = null;
        if (oAuthUser == null)
        {
            IEnumerable<string> claims = await adminApi.GetUserClaims(email, ct);
            if (!claims.Any())
            {
                var result = new SigninResponse(SigninStatus.Unauthorized, "L'usuari no està autoritzat");
                await ctx.Response.WriteAsJsonAsync(result, ct);
                return;
            }

            string? givenName = string.Empty;
            string? familyName = string.Empty;

            if (!fields.TryGetValue("given_name", out givenName) ||
                !fields.TryGetValue("family_name", out familyName))
            {
                var result = new SigninResponse(SigninStatus.Error, "Invalid token");
                await ctx.Response.WriteAsJsonAsync(result, ct);
                return;
            }

            OAuthUser newOAuthUser = new OAuthUser()
            {
                OAuthProviderCode = "google",
                Subject = subject,
                User = new User()
                {
                    Firstname = givenName,
                    Lastname = familyName,
                    Username = email,
                }
            };

            if (claims.Contains(RoleClaimValues.ADMIN))
            {
                newOAuthUser.User.UserClaims.Add(new UserClaim()
                {
                    Type = "role",
                    Value = RoleClaimValues.ADMIN,
                });
            }
            else if (claims.Contains(RoleClaimValues.READER))
            {
                newOAuthUser.User.UserClaims.Add(new UserClaim()
                {
                    Type = "role",
                    Value = RoleClaimValues.READER,
                });
            }

            await oAuthUsersRepository.InsertAsync(newOAuthUser, CancellationToken.None);
            user = newOAuthUser.User;
        }
        else
        {
            user = oAuthUser.User;

            // TODO: Clean
            // This updates the claims into db based on current google groups.
            var currentClaims = user.UserClaims.Where(x => x.Type == "role").ToList();
            currentClaims.RemoveAll(x => x.Type == "role");
            IEnumerable<string> claims = await adminApi.GetUserClaims(email, ct);
            foreach (var claim in claims)
            {
                currentClaims.Add(new UserClaim()
                {
                    Type = "role",
                    Value = claim,
                    User = oAuthUser.User,
                    UserId = oAuthUser.UserId,
                }); 
            }
            oAuthUser.User.UserClaims = currentClaims;
            await oAuthUsersRepository.UpdateAsync(oAuthUser, CancellationToken.None);
        }

        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user.ToClaimPrincipal());
        var res = new SigninResponse(SigninStatus.Ok);
        await ctx.Response.WriteAsJsonAsync(res, ct);
    }
}
