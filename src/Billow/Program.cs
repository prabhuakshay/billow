using Velopack;

namespace Billow;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Must run first: handles install, update and uninstall hooks from the installer.
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
