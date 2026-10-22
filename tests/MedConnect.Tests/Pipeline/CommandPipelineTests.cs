using MedConnect.Application.Pipeline;
using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Validation;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MedConnect.Tests.Pipeline;

public sealed class CommandPipelineTests
{
    [Fact]
    public async Task HandleAsync_WhenValidationFails_DoesNotCallHandler()
    {
        var handler = new RecordingHandler(
            Result<string>.Ok("wykonano"));

        var validator = new TestValidator(
            new RuleViolation(
                "NameRequired",
                "Nazwa jest wymagana."));

        var pipeline = CreatePipeline(handler, validator);

        var result = await pipeline.HandleAsync(
            new TestCommand(""));

        Assert.False(result.IsSuccess);
        Assert.IsType<Error.Validation>(result.Failure);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenValidationPasses_CallsHandler()
    {
        var handler = new RecordingHandler(
            Result<string>.Ok("wykonano"));

        var validator = new TestValidator();

        var pipeline = CreatePipeline(handler, validator);

        var command = new TestCommand("Test");

        using var cancellation = new CancellationTokenSource();

        var result = await pipeline.HandleAsync(
            command,
            cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal("wykonano", result.Value);

        Assert.Equal(1, handler.CallCount);
        Assert.Same(command, handler.ReceivedCommand);
        Assert.Equal(
            cancellation.Token,
            handler.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WhenHandlerReturnsConflict_PreservesError()
    {
        var expectedError = new Error.Conflict(
            "Operacja nie jest dozwolona.");

        var handler = new RecordingHandler(
            Result<string>.Fail(expectedError));

        var pipeline = CreatePipeline(
            handler,
            new TestValidator());

        var result = await pipeline.HandleAsync(
            new TestCommand("Test"));

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Failure);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyCanceled_DoesNotCallHandler()
    {
        var handler = new RecordingHandler(
            Result<string>.Ok("wykonano"));

        var pipeline = CreatePipeline(
            handler,
            new TestValidator());

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
            {
                await pipeline.HandleAsync(
                    new TestCommand("Test"),
                    cancellation.Token);
            });

        Assert.Equal(0, handler.CallCount);
    }

    private static CommandPipeline<TestCommand, string> CreatePipeline(
        RecordingHandler handler,
        TestValidator validator)
    {
        return new CommandPipeline<TestCommand, string>(
            handler,
            new ICommandValidator<TestCommand>[] { validator },
            NullLogger<CommandPipeline<TestCommand, string>>.Instance);
    }

    private sealed record TestCommand(string Name);

    private sealed class TestValidator(
        params RuleViolation[] errors)
        : ICommandValidator<TestCommand>
    {
        public IReadOnlyList<RuleViolation> Validate(
            TestCommand command)
        {
            return errors;
        }
    }

    private sealed class RecordingHandler(
        Result<string> result)
        : ICommandHandler<TestCommand, string>
    {
        public int CallCount { get; private set; }

        public TestCommand? ReceivedCommand { get; private set; }

        public CancellationToken ReceivedCancellationToken
        {
            get;
            private set;
        }

        public Task<Result<string>> HandleAsync(
            TestCommand command,
            CancellationToken ct = default)
        {
            CallCount++;

            ReceivedCommand = command;
            ReceivedCancellationToken = ct;

            return Task.FromResult(result);
        }
    }
}