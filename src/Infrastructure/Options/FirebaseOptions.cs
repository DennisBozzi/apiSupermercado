namespace ApiBozzis.Infrastructure.Options;

public sealed class FirebaseOptions
{
    public const string Section = "Firebase";
    public string ProjectId { get; set; } = string.Empty;
    public string StorageBucket { get; set; } = string.Empty;
    public string? CredentialsJson { get; set; }
}
