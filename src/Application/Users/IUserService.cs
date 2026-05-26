using ApiBozzis.Shared.Results;

namespace ApiBozzis.Application.Users;

public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
    Task<Result<UserResponse>> SetDocumentAsync(Guid id, SetDocumentRequest request, CancellationToken ct = default);
    Task<Result<UserResponse>> UpdatePhotoAsync(Guid id, UpdatePhotoRequest request, CancellationToken ct = default);
}
