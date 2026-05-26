using ApiSupermercado.Application.Abstractions;
using ApiSupermercado.Application.Abstractions.Auth;
using ApiSupermercado.Application.Abstractions.Repositories;
using ApiSupermercado.Application.Users;
using ApiSupermercado.Domain.Entities;
using ApiSupermercado.Shared.Results;
using Microsoft.Extensions.Logging;

namespace ApiSupermercado.Application.Auth;

internal sealed class AuthService : IAuthService
{
    private readonly IFirebaseAuthClient _firebase;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IFirebaseAuthClient firebase,
        IUserRepository users,
        IUnitOfWork uow,
        ILogger<AuthService> logger)
    {
        _firebase = firebase;
        _users = users;
        _uow = uow;
        _logger = logger;
    }

    public Task<Result<AuthSessionResponse>> SignInWithGoogleAsync(GoogleLoginRequest request, CancellationToken ct = default)
        => SignInWithFirebaseIdTokenAsync(request.IdToken, AuthProvider.Google, ct);

    public Task<Result<AuthSessionResponse>> VerifyMagicLinkAsync(MagicLinkVerifyRequest request, CancellationToken ct = default)
        => SignInWithFirebaseIdTokenAsync(request.IdToken, AuthProvider.EmailLink, ct);

    public Task<Result<AuthSessionResponse>> RestoreSessionAsync(RestoreSessionRequest request, CancellationToken ct = default)
        => SignInWithFirebaseIdTokenAsync(request.IdToken, providerHint: null, ct);

    public async Task<Result<MagicLinkSendResponse>> SendMagicLinkAsync(MagicLinkSendRequest request, CancellationToken ct = default)
    {
        await _firebase.GenerateSignInWithEmailLinkAsync(new EmailLinkRequest(request.Email, request.ContinueUrl), ct);
        return new MagicLinkSendResponse(true);
    }

    private async Task<Result<AuthSessionResponse>> SignInWithFirebaseIdTokenAsync(
        string idToken, AuthProvider? providerHint, CancellationToken ct)
    {
        FirebaseTokenInfo token;
        try
        {
            token = await _firebase.VerifyIdTokenAsync(idToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Firebase ID token verification failed (hint={Hint}).", providerHint);
            return Error.Unauthorized("auth.invalid_token", "Invalid or expired token.");
        }

        if (!token.EmailVerified)
            return Error.Unauthorized("auth.email_not_verified", "Email is not verified.");
        if (string.IsNullOrWhiteSpace(token.Email))
            return Error.Unauthorized("auth.no_email", "Token has no email claim.");

        var provider = providerHint ?? MapProvider(token.SignInProvider);

        var user = await _users.GetByEmailAsync(token.Email, ct);
        if (user is null)
        {
            user = User.Create(token.Uid, token.Email, token.EmailVerified, provider, token.Name, token.Picture);
            await _users.AddAsync(user, ct);
        }
        else
        {
            user.LinkProvider(token.Uid, provider);
            user.RegisterLogin(token.EmailVerified, token.Name, token.Picture);
            _users.Update(user);
        }

        await _uow.SaveChangesAsync(ct);
        return new AuthSessionResponse(user.ToResponse());
    }

    private static AuthProvider MapProvider(string signInProvider) => signInProvider switch
    {
        "google.com" => AuthProvider.Google,
        "emailLink" => AuthProvider.EmailLink,
        "password" => AuthProvider.Password,
        _ => AuthProvider.Unknown,
    };
}
