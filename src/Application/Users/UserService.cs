using ApiBozzis.Application.Abstractions;
using ApiBozzis.Application.Abstractions.Repositories;
using ApiBozzis.Application.Abstractions.Storage;
using ApiBozzis.Shared.Results;

namespace ApiBozzis.Application.Users;

internal sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storage;

    public UserService(IUserRepository users, IUnitOfWork uow, IStorageService storage)
    {
        _users = users;
        _uow = uow;
        _storage = storage;
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        return u is null
            ? Error.NotFound("user.not_found", "User not found.")
            : u.ToResponse();
    }

    public async Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        if (u is null) return Error.NotFound("user.not_found", "User not found.");

        u.UpdatePersonalInfo(request.Name, request.BirthDate);
        if (!string.IsNullOrWhiteSpace(request.PhotoUrl)) u.UpdateProfile(u.DisplayName, request.PhotoUrl);
        _users.Update(u);
        await _uow.SaveChangesAsync(ct);
        return u.ToResponse();
    }

    public async Task<Result<UserResponse>> SetDocumentAsync(Guid id, SetDocumentRequest request, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        if (u is null) return Error.NotFound("user.not_found", "User not found.");

        if (await _users.DocumentExistsAsync(request.Document, excludeUserId: id, ct: ct))
            return Error.Conflict("user.document_taken", "Document already in use.");

        u.SetDocument(request.Document, request.DocumentType);
        _users.Update(u);
        await _uow.SaveChangesAsync(ct);
        return u.ToResponse();
    }

    public async Task<Result<UserResponse>> UpdatePhotoAsync(Guid id, UpdatePhotoRequest request, CancellationToken ct = default)
    {
        var u = await _users.GetByIdAsync(id, ct);
        if (u is null) return Error.NotFound("user.not_found", "User not found.");

        var ext = Path.GetExtension(request.FileName);
        var objectName = $"users/{id:N}/avatar/{Guid.NewGuid():N}{ext}";
        var upload = await _storage.UploadAsync(
            new StorageUploadRequest(request.Content, objectName, request.ContentType, MakePublic: true), ct);

        var encodedObject = string.Join('/', upload.ObjectName.Split('/').Select(Uri.EscapeDataString));
        var publicUrl = $"https://storage.googleapis.com/{upload.Bucket}/{encodedObject}";
        u.UpdateProfile(u.DisplayName, publicUrl);
        _users.Update(u);
        await _uow.SaveChangesAsync(ct);
        return u.ToResponse();
    }
}
