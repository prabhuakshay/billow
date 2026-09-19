using Billow.Gst;

namespace Billow.Tests.Gst;

public class PanTests
{
    [Theory]
    [InlineData("ABCPK1234L")] // individual
    [InlineData("AAACB2894G")] // company
    [InlineData("AAAHR5678M")] // HUF
    [InlineData("AAPFU0939F")] // firm
    [InlineData("AAAAL4321B")] // association of persons
    [InlineData("AAATE1234F")] // trust
    [InlineData("AAABX9999Z")] // body of individuals
    [InlineData("AAALK0001A")] // local authority
    [InlineData("ABCJS1111Z")] // artificial juridical person
    [InlineData("AAAGD9876K")] // government
    public void AcceptsAValidPanForEveryHolderType(string input)
    {
        var result = Pan.Check(input);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal(input, result.Value!.Value);
    }

    [Theory]
    [InlineData("abcpk1234l")]
    [InlineData("  ABCPK1234L ")]
    [InlineData("\tAbcPk1234l\r\n")]
    public void NormalisesLowercaseAndPaddedInput(string input)
    {
        var result = Pan.Check(input);

        Assert.True(result.IsValid);
        Assert.Equal("ABCPK1234L", result.Value!.Value);
    }

    [Theory]
    [InlineData(null, "A PAN has 10 characters; this has 0.")]
    [InlineData("", "A PAN has 10 characters; this has 0.")]
    [InlineData("  ", "A PAN has 10 characters; this has 0.")]
    [InlineData("ABCPK1234", "A PAN has 10 characters; this has 9.")]
    [InlineData("ABCPK12345L", "A PAN has 10 characters; this has 11.")]
    public void RejectsTheWrongLength(string? input, string error)
    {
        AssertRejected(input, error);
    }

    [Theory]
    [InlineData("1BCPK1234L")] // digit among the first 5 letters
    [InlineData("ABCP11234L")]
    [InlineData("ABCPK123AL")] // letter among the 4 digits
    [InlineData("ABCPK12345")] // digit as the last letter
    [InlineData("ABCPK-234L")]
    [InlineData("ABCPKé234L")]
    [InlineData("ABCPK१२३४L")] // Devanagari digits
    public void RejectsABadPattern(string input)
    {
        AssertRejected(input, "A PAN is 5 letters, then 4 digits, then 1 letter.");
    }

    [Theory]
    [InlineData("ABCDK1234L")]
    [InlineData("ABCEK1234L")]
    [InlineData("ABCXK1234L")]
    [InlineData("ABCZK1234L")]
    public void RejectsAnUnknownHolderTypeLetter(string input)
    {
        AssertRejected(input, "The 4th letter of a PAN must be one of P, C, H, F, A, T, B, L, J or G.");
    }

    private static void AssertRejected(string? input, string error)
    {
        var result = Pan.Check(input);

        Assert.False(result.IsValid);
        Assert.Null(result.Value);
        Assert.Equal(error, result.Error);
    }
}
