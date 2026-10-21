using MedConnect.Core;
using MedConnect.Infrastructure.Database;
using MedConnect.Infrastructure.Security;
using MedConnect.Repositories;
using MedConnect.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
public static class ProgramExtensions
{
    public static IServiceCollection ConfigurationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        services.AddHostedService<RefreshTokenCleanupService>();

        services.AddControllers();
        services.AddOpenApi();

        return services;
    }

    public static WebApplication MapApplication(this WebApplication app)
    {
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        app.MapControllers();


        return app;
    }
}