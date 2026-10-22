using MedConnect.Application;
using MedConnect.Core;
using MedConnect.Extensions;
using MedConnect.Infrastructure;
using MedConnect.Infrastructure.Security;
using MedConnect.Middlewares;

public static class ProgramExtensions
{
    public static IServiceCollection ConfigurationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddApiDocumentation();
        services.AddAuthRateLimiting();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        services.AddTransient<RequestLoggingMiddleware>();
        services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddControllers();

        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        return services;
    }

    public static WebApplication MapApplication(
        this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();
        app.MapApiDocumentation();

        return app;
    }
}