using System.Text.RegularExpressions;

namespace Billow;

/// <summary>
/// A plausible email address: a name, @, then a domain with at least one dot. Whether the
/// address exists is never checked.
/// </summary>
public sealed partial class Email
{
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>The address as typed, trimmed.</summary>
    public string Value { get; }

    /// <summary>Trims <paramref name="input"/>, then checks it looks like an email address.</summary>
    public static CheckResult<Email> Check(string? input)
    {
        var email = (input ?? "").Trim();

        if (email.Length == 0)
        {
            return CheckResult<Email>.Invalid("Enter the email address.");
        }

        if (!Pattern().IsMatch(email))
        {
            return CheckResult<Email>.Invalid(
                "An email address is a name, then @, then a domain, such as sharma@gmail.com.");
        }

        return CheckResult<Email>.Valid(new Email(email));
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$")]
    private static partial Regex Pattern();
}
