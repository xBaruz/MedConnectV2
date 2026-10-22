using MedConnect.Validation;

namespace MedConnect.Core.Abstractions;

public interface ICommandValidator<in TCommand>
{
    IReadOnlyList<RuleViolation> Validate(TCommand command);
}