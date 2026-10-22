using MedConnect.Application.Auth;
using MedConnect.Application.Users;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Dtos;
using MedConnect.Core.Queries;

namespace MedConnect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddAuthHandlers();

        services.AddScoped<IQueryHandler<GetCurrentUserQuery, UserResponseDto>, GetCurrentUserHandler>();

        return services;
    }
}