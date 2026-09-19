namespace Billow.Gst;

/// <summary>A valid, normalised PAN (Permanent Account Number).</summary>
public sealed class Pan
{
    private const int Length = 10;

    /// <summary>
    /// The 4th letter says who holds the PAN: Person, Company, HUF, Firm, AOP, Trust, Body of
    /// individuals, Local authority, artificial Juridical person or Government.
    /// </summary>
    private const string HolderTypes = "PCHFATBLJG";

    private Pan(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>Trims and uppercases <paramref name="input"/>, then checks it is a valid PAN.</summary>
    public static CheckResult<Pan> Check(string? input)
    {
        var pan = (input ?? "").Trim().ToUpperInvariant();

        if (pan.Length != Length)
        {
            return CheckResult<Pan>.Invalid($"A PAN has {Length} characters; this has {pan.Length}.");
        }

        return CheckNormalised(pan);
    }

    /// <summary>Checks a PAN that is already the right length, trimmed and uppercased.</summary>
    internal static CheckResult<Pan> CheckNormalised(string pan)
    {
        var matchesPattern = pan[..5].All(char.IsAsciiLetterUpper)
            && pan[5..9].All(char.IsAsciiDigit)
            && char.IsAsciiLetterUpper(pan[9]);

        if (!matchesPattern)
        {
            return CheckResult<Pan>.Invalid("A PAN is 5 letters, then 4 digits, then 1 letter.");
        }

        if (!HolderTypes.Contains(pan[3], StringComparison.Ordinal))
        {
            return CheckResult<Pan>.Invalid(
                "The 4th letter of a PAN must be one of P, C, H, F, A, T, B, L, J or G.");
        }

        return CheckResult<Pan>.Valid(new Pan(pan));
    }

    public override string ToString() => Value;
}
