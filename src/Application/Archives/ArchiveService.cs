using ApiBozzis.Application.Abstractions;
using ApiBozzis.Application.Abstractions.Repositories;
using ApiBozzis.Application.Abstractions.Storage;
using ApiBozzis.Domain.Entities;
using ApiBozzis.Shared.Results;

namespace ApiBozzis.Application.Archives;

internal sealed class ArchiveService : IArchiveService
{
    private const int MaxPageSize = 100;

    private readonly IArchiveRepository _archives;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _uow;

    public ArchiveService(IArchiveRepository archives, IStorageService storage, IUnitOfWork uow)
    {
        _archives = archives;
        _storage = storage;
        _uow = uow;
    }

    public async Task<Result<ArchiveResponse>> UploadAsync(Guid ownerUserId, UploadArchiveRequest request, CancellationToken ct = default)
    {
        var objectName = $"users/{ownerUserId:N}/{Guid.NewGuid():N}/{request.FileName}";
        var upload = await _storage.UploadAsync(
            new StorageUploadRequest(request.Content, objectName, request.ContentType, request.IsPublic), ct);

        var archive = Archive.Create(
            ownerUserId, request.FileName, request.ContentType, upload.SizeBytes,
            upload.Bucket, upload.ObjectName, upload.Md5Hash, request.IsPublic);

        await _archives.AddAsync(archive, ct);
        await _uow.SaveChangesAsync(ct);
        return archive.ToResponse();
    }

    public async Task<Result<ArchiveResponse>> GetAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default)
    {
        var a = await _archives.GetByIdAsync(archiveId, ct);
        if (a is null) return Error.NotFound("archive.not_found", "Archive not found.");
        if (a.OwnerUserId != ownerUserId) return Error.Forbidden("archive.forbidden", "Access denied.");
        return a.ToResponse();
    }

    public async Task<Result<IReadOnlyList<ArchiveResponse>>> ListAsync(Guid ownerUserId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var items = await _archives.ListByOwnerAsync(ownerUserId, (page - 1) * pageSize, pageSize, ct);
        return Result.Success<IReadOnlyList<ArchiveResponse>>(items.Select(a => a.ToResponse()).ToList());
    }

    public async Task<Result> DeleteAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default)
    {
        var a = await _archives.GetByIdAsync(archiveId, ct);
        if (a is null) return Result.Failure(Error.NotFound("archive.not_found", "Archive not found."));
        if (a.OwnerUserId != ownerUserId) return Result.Failure(Error.Forbidden("archive.forbidden", "Access denied."));

        await _storage.DeleteAsync(a.Bucket, a.StorageObject, ct);
        _archives.Remove(a);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<SignedUrlResponse>> GetSignedUrlAsync(Guid ownerUserId, Guid archiveId, TimeSpan validity, CancellationToken ct = default)
    {
        var a = await _archives.GetByIdAsync(archiveId, ct);
        if (a is null) return Error.NotFound("archive.not_found", "Archive not found.");
        if (a.OwnerUserId != ownerUserId) return Error.Forbidden("archive.forbidden", "Access denied.");

        var url = await _storage.GetSignedUrlAsync(a.Bucket, a.StorageObject, validity, ct);
        return new SignedUrlResponse(url, DateTime.UtcNow.Add(validity));
    }

    public async Task<Result<(Stream Stream, string ContentType, string FileName)>> DownloadAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default)
    {
        var a = await _archives.GetByIdAsync(archiveId, ct);
        if (a is null) return Error.NotFound("archive.not_found", "Archive not found.");
        if (a.OwnerUserId != ownerUserId) return Error.Forbidden("archive.forbidden", "Access denied.");

        var stream = await _storage.DownloadAsync(a.Bucket, a.StorageObject, ct);
        return (stream, a.ContentType, a.FileName);
    }
}
