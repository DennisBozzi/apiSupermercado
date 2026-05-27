using ApiSupermercado.Infrastructure.Options;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace ApiSupermercado.Infrastructure.Firebase;

internal static class FirebaseAppFactory
{
    private static readonly object _lock = new();
    private const string AppName = "apiSupermercado";

    public static FirebaseApp GetOrCreate(FirebaseOptions options)
    {
        var existing = FirebaseApp.GetInstance(AppName);
        if (existing is not null) return existing;

        lock (_lock)
        {
            existing = FirebaseApp.GetInstance(AppName);
            if (existing is not null) return existing;

            return FirebaseApp.Create(new AppOptions
            {
                Credential = BuildCredential(options),
                ProjectId = options.ProjectId,
            }, AppName);
        }
    }

    public static GoogleCredential BuildCredential(FirebaseOptions options)
    {
        // Prefer file path: avoids env-var parsing issues with the JSON payload.
        if (!string.IsNullOrWhiteSpace(options.CredentialsPath))
        {
            var fullPath = Path.IsPathRooted(options.CredentialsPath)
                ? options.CredentialsPath
                : Path.GetFullPath(options.CredentialsPath);

            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException(
                    $"Firebase__CredentialsPath is set but file not found at '{fullPath}'. " +
                    "Place the service-account JSON at that path or update Firebase__CredentialsPath.");
            }

            return GoogleCredential.FromFile(fullPath);
        }

        if (!string.IsNullOrWhiteSpace(options.CredentialsJson))
        {
            return GoogleCredential.FromJson(options.CredentialsJson);
        }

        throw new InvalidOperationException(
            "Firebase credentials not configured. Set either Firebase__CredentialsPath " +
            "(path to the service-account JSON file — recommended) or Firebase__CredentialsJson " +
            "(inline JSON, single line). Get the file at Firebase Console -> Project Settings -> " +
            "Service accounts -> Generate new private key.");
    }
}
