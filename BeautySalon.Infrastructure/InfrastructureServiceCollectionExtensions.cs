using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BeautySalon.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<CurrentUserContext>();
        services.AddSingleton<ICurrentUserContext>(sp => sp.GetRequiredService<CurrentUserContext>());
        services.AddSingleton<ISessionService>(sp => sp.GetRequiredService<CurrentUserContext>());
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
