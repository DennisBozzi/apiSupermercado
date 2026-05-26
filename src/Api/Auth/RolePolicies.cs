using ApiBozzis.Api.Common;
using ApiBozzis.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ApiBozzis.Api.Auth;

public static class RolePolicies
{
    public const string Admin = "RequireAdmin";

    public static AuthorizationOptions AddRolePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(Admin, p => p
            .RequireAuthenticatedUser()
            .RequireClaim(ClaimsPrincipalExtensions.RoleClaim, ((int)ProfileType.Admin).ToString()));
        return options;
    }
}
