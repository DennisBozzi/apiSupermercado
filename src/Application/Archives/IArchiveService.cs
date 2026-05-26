using ApiSupermercado.Shared.Results;

namespace ApiSupermercado.Application.Archives;

public interface IArchiveService
{
    Task<Result<ArchiveResponse>> UploadAsync(Guid ownerUserId, UploadArchiveRequest request, CancellationToken ct = default);
    Task<Result<ArchiveResponse>> GetAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ArchiveResponse>>> ListAsync(Guid ownerUserId, int page, int pageSize, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default);
    Task<Result<SignedUrlResponse>> GetSignedUrlAsync(Guid ownerUserId, Guid archiveId, TimeSpan validity, CancellationToken ct = default);
    Task<Result<(Stream Stream, string ContentType, string FileName)>> DownloadAsync(Guid ownerUserId, Guid archiveId, CancellationToken ct = default);
}
