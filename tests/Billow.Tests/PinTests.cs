namespace Billow.Tests;

public class PinTests
{
    [Theory]
    [InlineData("411001", "411001")]
    [InlineData(" 411001 ", "411001")]
    [InlineData("\t110001\r\n", "110001")]
    public void AcceptsSixDigitsAndTrimsThem(string input, string expected)
    {
        var result = Pin.Check(input);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal(expected, result.Value!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AsksForAPinWhenBlank(string? input)
    {
        Assert.Equal("Enter the 6-digit PIN.", Pin.Check(input).Error);
    }

    [Theory]
    [InlineData("41100")]
    [InlineData("4110011")]
    [InlineData("41100A")]
    [InlineData("411 001")]
    [InlineData("４１１００１")]
    public void RejectsAnythingButExactlySixDigits(string input)
    {
        var result = Pin.Check(input);

        Assert.False(result.IsValid);
        Assert.Equal("A PIN is exactly 6 digits.", result.Error);
    }
}
