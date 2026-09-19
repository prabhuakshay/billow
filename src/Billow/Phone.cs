using System.Text.RegularExpressions;

namespace Billow;

/// <summary>
/// A plausible phone number: 8 to 13 digits, which may be grouped by spaces, hyphens and
/// brackets and start with +. Whether the number exists is never checked.
/// </summary>
public sealed partial class Phone
{
    private const int MinDigits = 8;
    private const int MaxDigits = 13;

    private Phone(string value)
    {
        Value = value;
    }

    /// <summary>The number as typed, trimmed.</summary>
    public string Value { get; }

    /// <summary>Trims <paramref name="input"/>, then checks it looks like a phone number.</summary>
    public static CheckResult<Phone> Check(string? input)
    {
        var phone = (input ?? "").Trim();

        if (phone.Length == 0)
        {
            return CheckResult<Phone>.Invalid("Enter the phone number.");
        }

        var digits = phone.Count(char.IsAsciiDigit);
        if (!Pattern().IsMatch(phone) || digits < MinDigits || digits > MaxDigits)
        {
            return CheckResult<Phone>.Invalid(
                $"A phone number is {MinDigits} to {MaxDigits} digits, with spaces, hyphens, brackets or a leading +, "
                + "such as 98765 43210.");
        }

        return CheckResult<Phone>.Valid(new Phone(phone));
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[0-9 ()-]+$")]
    private static partial Regex Pattern();
}
