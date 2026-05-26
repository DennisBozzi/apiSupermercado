using ApiBozzis.Shared.Results;

namespace ApiBozzis.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthSessionResponse>> SignInWithGoogleAsync(GoogleLoginRequest request, CancellationToken ct = default);
    Task<Result<MagicLinkSendResponse>> SendMagicLinkAsync(MagicLinkSendRequest request, CancellationToken ct = default);
    Task<Result<AuthSessionResponse>> VerifyMagicLinkAsync(MagicLinkVerifyRequest request, CancellationToken ct = default);
    Task<Result<AuthSessionResponse>> RestoreSessionAsync(RestoreSessionRequest request, CancellationToken ct = default);
}
