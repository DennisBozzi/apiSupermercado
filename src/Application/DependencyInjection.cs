using ApiBozzis.Application.Archives;
using ApiBozzis.Application.Auth;
using ApiBozzis.Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBozzis.Application;

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
