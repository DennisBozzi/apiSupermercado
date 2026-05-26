using ApiBozzis.Api.Common;
using ApiBozzis.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBozzis.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSessionResponse>> Google([FromBody] GoogleLoginRequest request, CancellationToken ct)
        => (await _auth.SignInWithGoogleAsync(request, ct)).ToActionResult();

    [HttpPost("magic-link/send")]
    [ProducesResponseType(typeof(MagicLinkSendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MagicLinkSendResponse>> SendMagicLink([FromBody] MagicLinkSendRequest request, CancellationToken ct)
        => (await _auth.SendMagicLinkAsync(request, ct)).ToActionResult();

    [HttpPost("magic-link/verify")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSessionResponse>> VerifyMagicLink([FromBody] MagicLinkVerifyRequest request, CancellationToken ct)
        => (await _auth.VerifyMagicLinkAsync(request, ct)).ToActionResult();

    [HttpPost("session")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSessionResponse>> RestoreSession([FromBody] RestoreSessionRequest request, CancellationToken ct)
        => (await _auth.RestoreSessionAsync(request, ct)).ToActionResult();
}
