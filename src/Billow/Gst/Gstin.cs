namespace Billow.Gst;

/// <summary>A valid, normalised GSTIN, from which the State code and PAN can be read.</summary>
public sealed class Gstin
{
    private const int Length = 15;
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int Radix = 36;

    private Gstin(string value, Pan pan)
    {
        Value = value;
        Pan = pan;
    }

    public string Value { get; }

    /// <summary>The two-digit GST state code the GSTIN starts with.</summary>
    public string StateCode => Value[..2];

    /// <summary>The PAN in characters 3–12.</summary>
    public Pan Pan { get; }

    /// <summary>Trims and uppercases <paramref name="input"/>, then checks it is a valid GSTIN.</summary>
    public static CheckResult<Gstin> Check(string? input)
    {
        var gstin = (input ?? "").Trim().ToUpperInvariant();

        if (gstin.Length != Length)
        {
            return CheckResult<Gstin>.Invalid($"A GSTIN has {Length} characters; this has {gstin.Length}.");
        }

        if (!gstin.All(c => char.IsAsciiLetterUpper(c) || char.IsAsciiDigit(c)))
        {
            return CheckResult<Gstin>.Invalid("A GSTIN can only contain the letters A–Z and the digits 0–9.");
        }

        if (!char.IsAsciiDigit(gstin[0]) || !char.IsAsciiDigit(gstin[1]))
        {
            return CheckResult<Gstin>.Invalid("A GSTIN starts with a 2-digit state code.");
        }

        // The 13th character numbers the registrations held under one PAN in one State, from 1.
        if (gstin[12] == '0')
        {
            return CheckResult<Gstin>.Invalid("The 13th character of a GSTIN must be 1–9 or A–Z.");
        }

        if (gstin[13] != 'Z')
        {
            return CheckResult<Gstin>.Invalid("The 14th character of a GSTIN must be Z.");
        }

        if (GstState.Find(gstin[..2]) is null)
        {
            return CheckResult<Gstin>.Invalid($"{gstin[..2]} is not a GST state code.");
        }

        var pan = Pan.CheckNormalised(gstin[2..12]);
        if (!pan.IsValid)
        {
            return CheckResult<Gstin>.Invalid($"Characters 3–12 of a GSTIN are its PAN. {pan.Error}");
        }

        if (gstin[14] != CheckCharacter(gstin[..14]))
        {
            return CheckResult<Gstin>.Invalid(
                "The last character of this GSTIN does not match the rest; check it for a typing mistake.");
        }

        return CheckResult<Gstin>.Valid(new Gstin(gstin, pan.Value!));
    }

    /// <summary>
    /// The standard GSTIN mod-36 check character. Each character counts as its position in
    /// 0–9A–Z; every second one is doubled, and a product of 36 or more has its base-36 digits
    /// added together.
    /// </summary>
    private static char CheckCharacter(string first14)
    {
        var sum = 0;
        for (var i = 0; i < first14.Length; i++)
        {
            var product = Alphabet.IndexOf(first14[i], StringComparison.Ordinal) * (i % 2 == 0 ? 1 : 2);
            sum += (product / Radix) + (product % Radix);
        }

        return Alphabet[(Radix - (sum % Radix)) % Radix];
    }

    public override string ToString() => Value;
}
