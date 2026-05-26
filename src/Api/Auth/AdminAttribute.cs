using Microsoft.AspNetCore.Authorization;

namespace ApiBozzis.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AdminAttribute : AuthorizeAttribute
{
    public AdminAttribute() : base(RolePolicies.Admin) { }
}
