using ApiBozzis.Infrastructure.Options;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace ApiBozzis.Infrastructure.Firebase;

internal static class FirebaseAppFactory
{
    private static readonly object _lock = new();
    private const string AppName = "apiBozzis";

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
        if (string.IsNullOrWhiteSpace(options.CredentialsJson))
        {
            throw new InvalidOperationException(
                "Firebase__CredentialsJson is not set. Paste the full service-account JSON " +
                "from Firebase Console -> Project Settings -> Service accounts -> Generate new private key, " +
                "as a single line (no surrounding quotes, no actual newlines — the \\n inside private_key stays literal).");
        }

        return GoogleCredential.FromJson(options.CredentialsJson);
    }
}
