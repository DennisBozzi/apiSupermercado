namespace ApiSupermercado.Domain.Entities;

public sealed class Archive
{
    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string StorageObject { get; private set; } = default!;
    public string Bucket { get; private set; } = default!;
    public string? Checksum { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Archive() { }

    public static Archive Create(
        Guid ownerUserId,
        string fileName,
        string contentType,
        long sizeBytes,
        string bucket,
        string storageObject,
        string? checksum,
        bool isPublic)
    {
        var now = DateTime.UtcNow;
        return new Archive
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Bucket = bucket,
            StorageObject = storageObject,
            Checksum = checksum,
            IsPublic = isPublic,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Rename(string fileName)
    {
        FileName = fileName;
        UpdatedAt = DateTime.UtcNow;
    }
}
