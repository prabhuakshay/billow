using Billow.Gst;

namespace Billow.Tests.Gst;

public class GstinTests
{
    [Theory]
    [InlineData("27AAPFU0939F1ZV", "27", "AAPFU0939F")] // Maharashtra, firm
    [InlineData("29AAACB2894G1ZJ", "29", "AAACB2894G")] // Karnataka, company
    [InlineData("07ABCPK1234L1ZU", "07", "ABCPK1234L")] // Delhi, individual
    [InlineData("09AAAHR5678M1ZE", "09", "AAAHR5678M")] // Uttar Pradesh, HUF
    [InlineData("26AAAGD9876K1ZU", "26", "AAAGD9876K")] // Dadra and Nagar Haveli and Daman and Diu, government
    [InlineData("38AAAAL4321B2Z5", "38", "AAAAL4321B")] // Ladakh, AOP, second registration
    [InlineData("97AAATE1234F1Z0", "97", "AAATE1234F")] // Other Territory, trust
    [InlineData("01ABCJS1111ZAZH", "01", "ABCJS1111Z")] // Jammu and Kashmir, artificial juridical person
    public void AcceptsAValidGstinAndReadsItsStateCodeAndPan(string input, string stateCode, string pan)
    {
        var result = Gstin.Check(input);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal(input, result.Value!.Value);
        Assert.Equal(stateCode, result.Value.StateCode);
        Assert.Equal(pan, result.Value.Pan.Value);
    }

    [Theory]
    [InlineData("27aapfu0939f1zv")]
    [InlineData("  27AAPFU0939F1ZV  ")]
    [InlineData("\t27aapfU0939F1zv\n")]
    public void NormalisesLowercaseAndPaddedInput(string input)
    {
        var result = Gstin.Check(input);

        Assert.True(result.IsValid);
        Assert.Equal("27AAPFU0939F1ZV", result.Value!.Value);
    }

    [Theory]
    [InlineData(null, "A GSTIN has 15 characters; this has 0.")]
    [InlineData("", "A GSTIN has 15 characters; this has 0.")]
    [InlineData("   ", "A GSTIN has 15 characters; this has 0.")]
    [InlineData("27AAPFU0939F1Z", "A GSTIN has 15 characters; this has 14.")]
    [InlineData("27AAPFU0939F1ZVV", "A GSTIN has 15 characters; this has 16.")]
    [InlineData("27 AAPFU0939F1ZV", "A GSTIN has 15 characters; this has 16.")]
    public void RejectsTheWrongLength(string? input, string error)
    {
        AssertRejected(input, error);
    }

    [Theory]
    [InlineData("27AAPFU0939F1Z#")]
    [InlineData("27AAPFU-939F1ZV")]
    [InlineData("27AAPFU0939F1Zé")]
    [InlineData("27AAPFU0939F1Z٣")] // Arabic-Indic digit three
    public void RejectsIllegalCharacters(string input)
    {
        AssertRejected(input, "A GSTIN can only contain the letters A–Z and the digits 0–9.");
    }

    [Theory]
    [InlineData("2AAAPFU0939F1ZV", "A GSTIN starts with a 2-digit state code.")]
    [InlineData("A7AAPFU0939F1ZV", "A GSTIN starts with a 2-digit state code.")]
    [InlineData("27AAPFU0939F0ZW", "The 13th character of a GSTIN must be 1–9 or A–Z.")]
    [InlineData("27AAPFU0939F1YX", "The 14th character of a GSTIN must be Z.")]
    public void RejectsCharactersOutOfPattern(string input, string error)
    {
        AssertRejected(input, error);
    }

    [Theory]
    [InlineData("00AAACB2894G1Z3", "00")]
    [InlineData("25AAACB2894G1ZR", "25")] // retired: Daman and Diu, merged into 26 in 2020
    [InlineData("28AAACB2894G1ZL", "28")] // retired: undivided Andhra Pradesh
    [InlineData("39AAACB2894G1ZI", "39")]
    [InlineData("99AAACB2894G1ZC", "99")]
    public void RejectsAnUnknownOrRetiredStateCode(string input, string stateCode)
    {
        AssertRejected(input, $"{stateCode} is not a GST state code.");
    }

    [Theory]
    [InlineData("27AAPF10939F1ZO", "A PAN is 5 letters, then 4 digits, then 1 letter.")]
    [InlineData("27AAPFUO939F1ZI", "A PAN is 5 letters, then 4 digits, then 1 letter.")] // letter O for zero
    [InlineData("27AAPFU093911ZN", "A PAN is 5 letters, then 4 digits, then 1 letter.")]
    [InlineData("27AAAXU0939F1Z9", "The 4th letter of a PAN must be one of P, C, H, F, A, T, B, L, J or G.")]
    public void RejectsAnInvalidEmbeddedPan(string input, string panError)
    {
        AssertRejected(input, $"Characters 3–12 of a GSTIN are its PAN. {panError}");
    }

    [Theory]
    [InlineData("27AAPFU0939F1Z3")] // should be letter V, given a digit
    [InlineData("27AAPFU0939F1ZW")] // should be letter V, given another letter
    [InlineData("07ABCPK1234L1ZV")] // should be letter U, given a neighbouring letter
    [InlineData("97AAATE1234F1Z1")] // should be digit 0, given another digit
    [InlineData("38AAAAL4321B2ZS")] // should be digit 5, given a letter
    [InlineData("27AAPFU0993F1ZV")] // two PAN digits swapped
    [InlineData("27AAACB2894G1ZJ")] // state code mistyped (29 as 27)
    [InlineData("29AAACB2894G2ZJ")] // registration number mistyped
    public void RejectsAWrongCheckDigit(string input)
    {
        AssertRejected(
            input,
            "The last character of this GSTIN does not match the rest; check it for a typing mistake.");
    }

    private static void AssertRejected(string? input, string error)
    {
        var result = Gstin.Check(input);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(error, result.Error);
    }
}
