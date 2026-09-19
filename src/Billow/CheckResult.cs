namespace Billow;

/// <summary>
/// The outcome of checking a typed-in identifier: either the valid, normalised value or the
/// reason it is invalid, worded to be shown next to the field.
/// </summary>
public sealed class CheckResult<T>
    where T : class
{
    private CheckResult(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public string? Error { get; }

    public bool IsValid => Value is not null;

    internal static CheckResult<T> Valid(T value) => new(value, null);

    internal static CheckResult<T> Invalid(string error) => new(null, error);
}
