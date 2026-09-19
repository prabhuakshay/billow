namespace Billow.Tests;

public class PhoneTests
{
    [Theory]
    [InlineData("9876543210", "9876543210")]
    [InlineData(" 98765 43210 ", "98765 43210")]
    [InlineData("+91 98765 43210", "+91 98765 43210")]
    [InlineData("020-2612 3456", "020-2612 3456")]
    [InlineData("(020) 26123456", "(020) 26123456")]
    [InlineData("26123456", "26123456")]
    public void AcceptsAPlausibleNumberAndTrimsIt(string input, string expected)
    {
        var result = Phone.Check(input);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal(expected, result.Value!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AsksForANumberWhenBlank(string? input)
    {
        Assert.Equal("Enter the phone number.", Phone.Check(input).Error);
    }

    [Theory]
    [InlineData("1234567")] // too few digits
    [InlineData("12345678901234")] // too many digits
    [InlineData("98765O4321")] // letter O for zero
    [InlineData("98765 43210 ext 2")]
    [InlineData("91+9876543210")] // + only at the start
    [InlineData("９８７６５４３２１０")] // full-width digits
    public void RejectsAnythingThatDoesNotLookLikeAPhoneNumber(string input)
    {
        var result = Phone.Check(input);

        Assert.False(result.IsValid);
        Assert.Equal(
            "A phone number is 8 to 13 digits, with spaces, hyphens, brackets or a leading +, such as 98765 43210.",
            result.Error);
    }
}
