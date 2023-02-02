using System.Security.Claims;

namespace WebServer.Services;

public class CurrentRequestService : Application.Common.Services.ICurrentRequestService
{

    private readonly long? _userId;

    public CurrentRequestService(IHttpContextAccessor httpCtxAccessor)
    {
        _userId = GetUserIdFromHttpContext(httpCtxAccessor.HttpContext);
    }

    public long? UserId => _userId;

    private static long? GetUserIdFromHttpContext(HttpContext? ctx)
    {
        if (ctx != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
        {
            string? userIdValue = ctx.User.GetUserId();
            long userId;
            if (userIdValue != null && long.TryParse(userIdValue, out userId))
            {
                return userId;
            }
        }
        return null;
    }
}

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? claim.Value : null;
    }
}