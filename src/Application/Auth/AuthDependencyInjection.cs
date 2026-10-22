using MedConnect.Application.Pipeline;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Core.Dtos;

namespace MedConnect.Application.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthHandlers(
        this IServiceCollection services)
    {
        services.AddScoped<ICommandValidator<RegisterUserCommand>,RegisterUserValidator>();

        services.AddScoped<ICommandValidator<LoginCommand>,LoginValidator>();

        services.AddScoped<ICommandValidator<RefreshTokenCommand>,RefreshTokenValidator>();

        AddCommand<RegisterUserCommand,AuthResponseDto,RegisterUserHandler>(services);

        AddCommand<LoginCommand,AuthResponseDto,LoginHandler>(services);

        AddCommand<RefreshTokenCommand,AuthResponseDto,RefreshTokenHandler>(services);

        return services;
    }

    private static void AddCommand<TCommand, TResult, THandler>(
        IServiceCollection services)
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<THandler>();

        services.AddScoped<ICommandHandler<TCommand, TResult>>(
            provider =>
                new CommandPipeline<TCommand, TResult>(
                    provider.GetRequiredService<THandler>(),

                    provider.GetServices<ICommandValidator<TCommand>>(),

                    provider.GetRequiredService<ILogger<CommandPipeline<TCommand, TResult>>>()));
    }
}