using ApiBozzis.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiBozzis.Infrastructure.Firebase;

// Fails the host startup if the Firebase credential can't be exchanged for a
// token. Without this, the first auth request would hang for ~100s on the
// default Google HttpClient timeout.
internal sealed class FirebaseStartupValidator : IHostedService
{
    private readonly IOptions<FirebaseOptions> _options;
    private readonly ILogger<FirebaseStartupValidator> _logger;

    public FirebaseStartupValidator(
        IOptions<FirebaseOptions> options,
        ILogger<FirebaseStartupValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var opts = _options.Value;
        try
        {
            FirebaseAppFactory.GetOrCreate(opts);
            var credential = FirebaseAppFactory.BuildCredential(opts);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            var token = await credential
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform")
                .UnderlyingCredential
                .GetAccessTokenForRequestAsync(cancellationToken: cts.Token);

            _logger.LogInformation(
                "Firebase initialized. ProjectId={ProjectId}, Bucket={Bucket}, TokenLen={TokenLen}",
                opts.ProjectId, opts.StorageBucket, token.Length);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "Failed to initialize Firebase at startup. ProjectId={ProjectId}.",
                opts.ProjectId);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
