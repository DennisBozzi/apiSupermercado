using ApiSupermercado.Application.Abstractions;
using ApiSupermercado.Application.Abstractions.Auth;
using ApiSupermercado.Application.Abstractions.Repositories;
using ApiSupermercado.Application.Abstractions.Storage;
using ApiSupermercado.Infrastructure.Firebase;
using ApiSupermercado.Infrastructure.Options;
using ApiSupermercado.Infrastructure.Persistence;
using ApiSupermercado.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSupermercado.Infrastructure;

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
