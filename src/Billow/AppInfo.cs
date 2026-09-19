using System.Reflection;

namespace Billow;

public static class AppInfo
{
    /// <summary>
    /// Full version (e.g. 0.2.0-beta.1) as set by -p:Version. The SDK appends "+&lt;commit&gt;"
    /// to InformationalVersion, which we drop.
    /// </summary>
    public static string Version { get; } = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion.Split('+')[0] ?? "unknown";
}
