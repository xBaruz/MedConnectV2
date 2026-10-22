using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MedConnect.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            AddPolicy(options, AuthRateLimitPolicies.Login, permitLimit: 10);
            AddPolicy(options, AuthRateLimitPolicies.Register, permitLimit: 3);
            AddPolicy(options, AuthRateLimitPolicies.RefreshToken, permitLimit: 20);

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter,out var retryAfter))
                {
                    var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                    context.HttpContext.Response.Headers["Retry-After"] = seconds.ToString(CultureInfo.InvariantCulture);
                }

                await Results.Problem(statusCode: StatusCodes.Status429TooManyRequests, title: "Przekroczono limit żądań.", detail: "Spróbuj ponownie za chwilę.").ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    private static void AddPolicy(RateLimiterOptions options, string policyName, int permitLimit)
    {
        options.AddPolicy<string>(policyName, context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress;
                var clientKey = ipAddress is null ? "unknown" : ipAddress.MapToIPv6().ToString();

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey: clientKey, factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });
    }
}