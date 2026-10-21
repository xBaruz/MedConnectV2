namespace MedConnect.Validation;

public sealed record RuleViolation(string Code, string Message);

public sealed class RuleSet<T>
{
    private readonly List<(Func<T, bool> Predicate, RuleViolation Violation)> _rules = new();

    public RuleSet<T> AddRule(Func<T, bool> predicate, RuleViolation violation)
    {
        _rules.Add((predicate, violation));
        return this;
    }

    public RuleSet<T> When(Func<T, bool> condition, Action<RuleSet<T>> rules)
    {
        var conditionalRuleSet = new RuleSet<T>();
        rules(conditionalRuleSet);

        foreach (var (predicate, violation) in conditionalRuleSet.Rules)
        {
            _rules.Add((item => !condition(item) || predicate(item), violation));
        }
        return this;
    }

    public IReadOnlyList<(Func<T, bool> Predicate, RuleViolation Violation)> Rules => _rules;

    public List<RuleViolation> Validate(T target) 
        => _rules.Where(r => !r.Predicate(target)).Select(r => r.Violation).ToList();
}