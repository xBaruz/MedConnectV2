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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nieobsłużony wyjątek dla {Method} {Path}. TraceId: {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Wystąpił nieoczekiwany błąd.", extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = context.TraceIdentifier
                }
            ).ExecuteAsync(context);
        }
    }
}

public sealed class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (context.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("{Method} {Path}: żądanie przerwane po {Elapsed} ms", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation("{Method} {Path} -> {Status} w {Elapsed} ms", context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}