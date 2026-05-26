using ApiSupermercado.Application.Archives;
using ApiSupermercado.Application.Auth;
using ApiSupermercado.Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSupermercado.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IArchiveService, ArchiveService>();

        services.AddValidatorsFromAssemblyContaining<GoogleLoginRequestValidator>(includeInternalTypes: true);
        return services;
    }
}
