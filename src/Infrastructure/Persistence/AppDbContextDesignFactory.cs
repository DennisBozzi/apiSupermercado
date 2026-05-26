using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiSupermercado.Infrastructure.Persistence;

// Used by `dotnet ef`. Loads .env here because Program.cs isn't executed at
// design time. No fallback connection string — fail fast over silently writing
// to the wrong server.
internal sealed class AppDbContextDesignFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // .env may contain values DotNetEnv cannot parse (e.g. inline JSON for
        // Firebase). We only need the connection string here — try loading and
        // silently ignore parse errors so migrations don't break.
        try { Env.TraversePath().Load(); } catch { /* ignore .env parse errors */ }

        var conn = Environment.GetEnvironmentVariable("Database__ConnectionString");
        if (string.IsNullOrWhiteSpace(conn))
        {
            throw new InvalidOperationException(
                "Database__ConnectionString is not set. Either export it in your shell or fix the .env file " +
                "(common cause: unquoted JSON in Firebase__CredentialsJson breaks the parser).");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conn)
            .Options;

        return new AppDbContext(options);
    }
}
