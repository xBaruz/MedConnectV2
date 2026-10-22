using System.Diagnostics;
using MedConnect.Core;
using MedConnect.Core.Abstractions;

namespace MedConnect.Application.Pipeline;

public sealed class CommandPipeline<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> inner,
    IEnumerable<ICommandValidator<TCommand>> validators,
    ILogger<CommandPipeline<TCommand, TResult>> logger)
    : ICommandHandler<TCommand, TResult>
{
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var violations = validators
                .SelectMany(validator => validator.Validate(command))
                .ToArray();

            if (violations.Length > 0)
            {
                logger.LogInformation("Walidacja {Command} odrzucona. Liczba błędów: {Count}", typeof(TCommand).Name, violations.Length);

                var message = string.Join("; ", violations.Select(violation => violation.Message));

                return Result<TResult>.Fail(new Error.Validation(message));
            }

            var result = await inner.HandleAsync(command, ct);

            logger.LogInformation("Obsłużono {Command}. Sukces: {Success}", typeof(TCommand).Name, result.IsSuccess);

            return result;
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation("{Command} zakończono po {Elapsed} ms", typeof(TCommand).Name, stopwatch.ElapsedMilliseconds);
        }
    }
}