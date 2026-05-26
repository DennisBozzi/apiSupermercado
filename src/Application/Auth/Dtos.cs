using ApiSupermercado.Application.Users;

namespace ApiSupermercado.Application.Auth;

public sealed record GoogleLoginRequest(string IdToken);
public sealed record MagicLinkSendRequest(string Email, string ContinueUrl);
public sealed record MagicLinkSendResponse(bool Sent);
public sealed record MagicLinkVerifyRequest(string IdToken);
public sealed record RestoreSessionRequest(string IdToken);

public sealed record AuthSessionResponse(UserResponse User);
