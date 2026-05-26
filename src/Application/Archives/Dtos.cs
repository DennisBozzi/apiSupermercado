using ApiSupermercado.Domain.Entities;

namespace ApiSupermercado.Application.Archives;

public sealed record ArchiveResponse(
    Guid Id,
    Guid OwnerUserId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAt);

public sealed record UploadArchiveRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    bool IsPublic);

public sealed record SignedUrlResponse(string Url, DateTime ExpiresAt);

internal static class ArchiveMappings
{
    public static ArchiveResponse ToResponse(this Archive a) => new(
        a.Id, a.OwnerUserId, a.FileName, a.ContentType, a.SizeBytes, a.CreatedAt);
}
