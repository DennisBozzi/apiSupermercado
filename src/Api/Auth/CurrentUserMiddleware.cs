using ApiBozzis.Api.Common;
using ApiBozzis.Application.Abstractions.Repositories;
using System.Security.Claims;

namespace ApiBozzis.Api.Auth;

public sealed class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;
    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IUserRepository users)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var firebaseUid = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("user_id")?.Value
                ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(firebaseUid))
            {
                var u = await users.GetByFirebaseUidAsync(firebaseUid, context.RequestAborted);
                if (u is not null)
                {
                    var identity = (ClaimsIdentity)context.User.Identity!;
                    identity.AddClaim(new Claim(ClaimsPrincipalExtensions.InternalUserIdClaim, u.Id.ToString()));
                    foreach (var role in u.Roles)
                        identity.AddClaim(new Claim(ClaimsPrincipalExtensions.RoleClaim, role.ToString()));
                }
            }
        }
        await _next(context);
    }
}
