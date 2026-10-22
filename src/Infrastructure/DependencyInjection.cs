using MedConnect.Infrastructure.Database;
using MedConnect.Infrastructure.Security;
using MedConnect.Repositories;
using MedConnect.Services;
using Microsoft.EntityFrameworkCore;
using MedConnect.Core.Abstractions;

namespace MedConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Brak konfiguracji ConnectionStrings:DefaultConnection.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUserReadRepository, UserReadRepository>();

        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHostedService<RefreshTokenCleanupService>();
        
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }
}