using System.Diagnostics;

namespace MedConnect.Middlewares;

public sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nieobsłużony wyjątek dla {Method} {Path}", context.Request.Method, context.Request.Path);

            await Results.Problem(title: "Coś się nie udało", statusCode: 500)
                .ExecuteAsync(context);
        }
    }
}

public sealed class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var watch = Stopwatch.StartNew();

        await next(context);

        logger.LogInformation("{Method} {Path} -> {Status} w {Elapsed} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            watch.ElapsedMilliseconds);
    }
}