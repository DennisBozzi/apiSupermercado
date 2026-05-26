namespace ApiSupermercado.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string Section = "Database";
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 5;
}
