namespace ApiBozzis.Application.Abstractions.Auth;

public sealed record FirebaseTokenInfo(
    string Uid,
    string Email,
    bool EmailVerified,
    string? Name,
    string? Picture,
    string SignInProvider);

public sealed record EmailLinkRequest(string Email, string ContinueUrl);
public sealed record EmailLinkResult(string Link);

public interface IFirebaseAuthClient
{
    Task<FirebaseTokenInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
    Task<EmailLinkResult> GenerateSignInWithEmailLinkAsync(EmailLinkRequest request, CancellationToken ct = default);
}
