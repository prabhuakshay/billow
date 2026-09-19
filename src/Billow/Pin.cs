namespace Billow;

/// <summary>A valid Indian PIN (postal code): exactly six digits.</summary>
public sealed class Pin
{
    private const int Length = 6;

    private Pin(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>Trims <paramref name="input"/>, then checks it is a valid PIN.</summary>
    public static CheckResult<Pin> Check(string? input)
    {
        var pin = (input ?? "").Trim();

        if (pin.Length == 0)
        {
            return CheckResult<Pin>.Invalid($"Enter the {Length}-digit PIN.");
        }

        if (pin.Length != Length || !pin.All(char.IsAsciiDigit))
        {
            return CheckResult<Pin>.Invalid($"A PIN is exactly {Length} digits.");
        }

        return CheckResult<Pin>.Valid(new Pin(pin));
    }

    public override string ToString() => Value;
}
