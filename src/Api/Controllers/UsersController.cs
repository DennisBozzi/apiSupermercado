using ApiSupermercado.Api.Common;
using ApiSupermercado.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiSupermercado.Api.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private const long MaxPhotoBytes = 5L * 1024 * 1024;

    private readonly IUserService _users;
    public UsersController(IUserService users) => _users = users;

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
        => (await _users.GetByIdAsync(User.GetUserId(), ct)).ToActionResult();

    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
        => (await _users.UpdateProfileAsync(User.GetUserId(), request, ct)).ToActionResult();

    [HttpPut("me/document")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> SetDocument([FromBody] SetDocumentRequest request, CancellationToken ct)
        => (await _users.SetDocumentAsync(User.GetUserId(), request, ct)).ToActionResult();

    [HttpPut("me/photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxPhotoBytes)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> UpdatePhoto([FromForm] UpdatePhotoForm form, CancellationToken ct)
    {
        var file = form.File;
        if (file is null || file.Length == 0) return BadRequest("File is required.");
        if (file.Length > MaxPhotoBytes) return BadRequest("File too large (max 5 MB).");
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only image/* content types are allowed.");

        await using var stream = file.OpenReadStream();
        var result = await _users.UpdatePhotoAsync(
            User.GetUserId(),
            new UpdatePhotoRequest(stream, Path.GetFileName(file.FileName), file.ContentType, file.Length),
            ct);
        return result.ToActionResult();
    }

    public sealed class UpdatePhotoForm
    {
        public IFormFile File { get; set; } = default!;
    }
}
