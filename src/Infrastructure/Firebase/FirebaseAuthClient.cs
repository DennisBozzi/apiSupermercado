using ApiBozzis.Application.Abstractions.Auth;
using ApiBozzis.Infrastructure.Options;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Options;

namespace ApiBozzis.Infrastructure.Firebase;

internal sealed class FirebaseAuthClient : IFirebaseAuthClient
{
    private readonly FirebaseAuth _auth;

    public FirebaseAuthClient(IOptions<FirebaseOptions> options)
    {
        var app = FirebaseAppFactory.GetOrCreate(options.Value);
        _auth = FirebaseAuth.GetAuth(app);
    }

    public async Task<FirebaseTokenInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        var decoded = await _auth.VerifyIdTokenAsync(idToken, checkRevoked: false, ct);
        var claims = decoded.Claims;

        var email = ReadString(claims, "email") ?? string.Empty;
        var emailVerified = ReadBool(claims, "email_verified");
        var name = ReadString(claims, "name");
        var picture = ReadString(claims, "picture");
        var signInProvider = "unknown";
        if (claims.TryGetValue("firebase", out var firebaseRaw) && firebaseRaw is IDictionary<string, object> fb
            && fb.TryGetValue("sign_in_provider", out var sip) && sip is string s) signInProvider = s;

        return new FirebaseTokenInfo(decoded.Uid, email, emailVerified, name, picture, signInProvider);
    }

    public async Task<EmailLinkResult> GenerateSignInWithEmailLinkAsync(EmailLinkRequest request, CancellationToken ct = default)
    {
        var settings = new ActionCodeSettings
        {
            Url = request.ContinueUrl,
            HandleCodeInApp = true,
        };
        var link = await _auth.GenerateSignInWithEmailLinkAsync(request.Email, settings);
        return new EmailLinkResult(link);
    }

    private static string? ReadString(IReadOnlyDictionary<string, object> claims, string key)
        => claims.TryGetValue(key, out var v) && v is string s ? s : null;

    private static bool ReadBool(IReadOnlyDictionary<string, object> claims, string key)
        => claims.TryGetValue(key, out var v) && v is bool b && b;
}
