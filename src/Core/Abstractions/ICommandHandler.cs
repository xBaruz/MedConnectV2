namespace MedConnect.Core.Abstractions;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> HandleAsync(
        TCommand command,
        CancellationToken ct = default);
}