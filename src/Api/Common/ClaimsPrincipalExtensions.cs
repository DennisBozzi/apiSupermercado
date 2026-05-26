using System.Security.Claims;

namespace ApiBozzis.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public const string InternalUserIdClaim = "internal_user_id";
    public const string RoleClaim = "app_role";

    public static IReadOnlyList<int> GetRoles(this ClaimsPrincipal user) =>
        user.FindAll(RoleClaim)
            .Select(c => int.TryParse(c.Value, out var v) ? v : -1)
            .Where(v => v >= 0)
            .ToList();

    public static bool HasRole(this ClaimsPrincipal user, int role) =>
        user.FindAll(RoleClaim).Any(c => c.Value == role.ToString());

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(InternalUserIdClaim)?.Value;
        return Guid.TryParse(raw, out var id)
            ? id
            : throw new UnauthorizedAccessException("Internal user id claim missing.");
    }

    public static string GetFirebaseUid(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("user_id")?.Value
            ?? user.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Firebase uid claim missing.");
}
