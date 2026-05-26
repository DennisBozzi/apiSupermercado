using ApiBozzis.Application.Abstractions.Storage;
using ApiBozzis.Infrastructure.Options;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace ApiBozzis.Infrastructure.Firebase;

internal sealed class FirebaseStorageService : IStorageService
{
    private readonly StorageClient _client;
    private readonly UrlSigner _signer;
    private readonly string _bucket;

    public FirebaseStorageService(IOptions<FirebaseOptions> options)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.StorageBucket))
            throw new InvalidOperationException("Firebase StorageBucket is not configured.");

        var credential = FirebaseAppFactory.BuildCredential(opts);
        _client = StorageClient.Create(credential);
        _bucket = opts.StorageBucket;

        var underlying = credential.UnderlyingCredential;
        _signer = underlying is ServiceAccountCredential sa
            ? UrlSigner.FromCredential(sa)
            : UrlSigner.FromCredential(credential);
    }

    public async Task<StorageUploadResult> UploadAsync(StorageUploadRequest request, CancellationToken ct = default)
    {
        var obj = new Google.Apis.Storage.v1.Data.Object
        {
            Bucket = _bucket,
            Name = request.ObjectName,
            ContentType = request.ContentType,
        };
        var uploaded = await _client.UploadObjectAsync(obj, request.Content, cancellationToken: ct);
        if (request.MakePublic)
        {
            uploaded.Acl ??= [];
            uploaded.Acl.Add(new Google.Apis.Storage.v1.Data.ObjectAccessControl
            {
                Entity = "allUsers",
                Role = "READER",
            });
            uploaded = await _client.UpdateObjectAsync(uploaded, cancellationToken: ct);
        }
        return new StorageUploadResult(
            _bucket,
            uploaded.Name,
            (long)(uploaded.Size ?? 0UL),
            uploaded.Md5Hash);
    }

    public Task DeleteAsync(string bucket, string objectName, CancellationToken ct = default)
        => _client.DeleteObjectAsync(bucket, objectName, cancellationToken: ct);

    public Task<string> GetSignedUrlAsync(string bucket, string objectName, TimeSpan validity, CancellationToken ct = default)
        => _signer.SignAsync(bucket, objectName, validity, HttpMethod.Get, cancellationToken: ct);

    public async Task<Stream> DownloadAsync(string bucket, string objectName, CancellationToken ct = default)
    {
        var stream = new MemoryStream();
        await _client.DownloadObjectAsync(bucket, objectName, stream, cancellationToken: ct);
        stream.Position = 0;
        return stream;
    }
}
