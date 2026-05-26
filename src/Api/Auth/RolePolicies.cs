using ApiSupermercado.Api.Common;
using ApiSupermercado.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ApiSupermercado.Api.Auth;

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
