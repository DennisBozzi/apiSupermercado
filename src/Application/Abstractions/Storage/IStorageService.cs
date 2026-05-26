namespace ApiBozzis.Application.Abstractions.Storage;

public sealed record StorageUploadRequest(
    Stream Content,
    string ObjectName,
    string ContentType,
    bool MakePublic);

public sealed record StorageUploadResult(
    string Bucket,
    string ObjectName,
    long SizeBytes,
    string? Md5Hash);

public interface IStorageService
{
    Task<StorageUploadResult> UploadAsync(StorageUploadRequest request, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string objectName, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string bucket, string objectName, TimeSpan validity, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string bucket, string objectName, CancellationToken ct = default);
}
