namespace Billow.Tests;

public class AppInfoTests
{
    [Fact]
    public void VersionIsReadFromTheAppAssembly()
    {
        Assert.NotEqual("unknown", AppInfo.Version);
    }

    [Fact]
    public void VersionDropsTheCommitSuffix()
    {
        Assert.DoesNotContain("+", AppInfo.Version, StringComparison.Ordinal);
    }
}
