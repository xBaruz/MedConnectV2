namespace MedConnect.Core;

public readonly record struct Result<T>(bool IsSuccess, T? Value, Error? Failure)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(Error error) => new(false, default, error);

    public TResult Match<TResult>(Func<T, TResult> success, Func<Error, TResult> failure) 
        => IsSuccess ? success(Value!) : failure(Failure!);
}

public abstract record Error(string Code, string Message)
{
    public sealed record NotFound(string EntityName, Guid Id) 
        : Error("NotFound", $"{EntityName} with ID {Id} was not found.");
        
    public sealed record Validation(string Message) 
        : Error("ValidationError", Message);
        
    public sealed record Conflict(string Message) 
        : Error("ConflictError", Message);
        
    public sealed record Unauthorized(string Message = "Invalid credentials.") 
        : Error("UnauthorizedError", Message);
}