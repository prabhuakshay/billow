namespace Billow.Tests;

public class EmailTests
{
    [Theory]
    [InlineData("sharma@gmail.com", "sharma@gmail.com")]
    [InlineData(" Accounts.Pune@Sharma-Stores.co.in ", "Accounts.Pune@Sharma-Stores.co.in")]
    [InlineData("a+bills@example.org", "a+bills@example.org")]
    public void AcceptsAPlausibleAddressAndTrimsIt(string input, string expected)
    {
        var result = Email.Check(input);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Equal(expected, result.Value!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AsksForAnAddressWhenBlank(string? input)
    {
        Assert.Equal("Enter the email address.", Email.Check(input).Error);
    }

    [Theory]
    [InlineData("sharma")]
    [InlineData("sharma@")]
    [InlineData("@gmail.com")]
    [InlineData("sharma@gmail")]
    [InlineData("sharma@gmail.")]
    [InlineData("sharma@@gmail.com")]
    [InlineData("sharma @gmail.com")]
    [InlineData("sharma@gmail..com")]
    public void RejectsAnythingThatDoesNotLookLikeAnEmailAddress(string input)
    {
        var result = Email.Check(input);

        Assert.False(result.IsValid);
        Assert.Equal("An email address is a name, then @, then a domain, such as sharma@gmail.com.", result.Error);
    }
}
