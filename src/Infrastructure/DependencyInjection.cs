using ApiBozzis.Application.Abstractions;
using ApiBozzis.Application.Abstractions.Auth;
using ApiBozzis.Application.Abstractions.Repositories;
using ApiBozzis.Application.Abstractions.Storage;
using ApiBozzis.Infrastructure.Firebase;
using ApiBozzis.Infrastructure.Options;
using ApiBozzis.Infrastructure.Persistence;
using ApiBozzis.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBozzis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.Section));
        services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.Section));

        var db = configuration.GetSection(DatabaseOptions.Section).Get<DatabaseOptions>() ?? new DatabaseOptions();
        if (string.IsNullOrWhiteSpace(db.ConnectionString))
            throw new InvalidOperationException("Database:ConnectionString is not configured.");

        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(db.ConnectionString, npg =>
            {
                if (db.EnableRetryOnFailure)
                    npg.EnableRetryOnFailure(db.MaxRetryCount);
            });
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IArchiveRepository, ArchiveRepository>();

        services.AddSingleton<IFirebaseAuthClient, FirebaseAuthClient>();
        services.AddSingleton<IStorageService, FirebaseStorageService>();
        services.AddHostedService<Firebase.FirebaseStartupValidator>();

        return services;
    }
}
