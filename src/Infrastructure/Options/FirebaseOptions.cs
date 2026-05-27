namespace ApiSupermercado.Infrastructure.Options;

public sealed class FirebaseOptions
{
    public const string Section = "Firebase";
    public string ProjectId { get; set; } = string.Empty;
    public string StorageBucket { get; set; } = string.Empty;

    /// <summary>
    /// Path to the service-account JSON file. Preferred over <see cref="CredentialsJson"/>.
    /// Absolute paths used as-is; relative paths resolved against the process working directory.
    /// </summary>
    public string? CredentialsPath { get; set; }

    /// <summary>
    /// Inline service-account JSON (single line). Fallback when <see cref="CredentialsPath"/> is not set.
    /// </summary>
    public string? CredentialsJson { get; set; }
}
