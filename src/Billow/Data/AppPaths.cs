using System.IO;

namespace Billow.Data;

/// <summary>
/// Where Billow keeps its files. Data must NOT live under %LocalAppData%\Billow: that is the
/// installer's folder, which is wiped on install, reinstall and uninstall.
/// </summary>
public static class AppPaths
{
    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BillowData");

    public static string DatabasePath { get; } = Path.Combine(DataDirectory, "billow.db");
}
