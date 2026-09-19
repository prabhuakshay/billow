using System.Windows;
using Billow.BusinessDetails;
using Billow.Data;
using Microsoft.EntityFrameworkCore;

namespace Billow;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create the database on first run, and apply any schema changes after an update.
        using (var db = new BillowDbContext())
        {
            db.Database.Migrate();
        }

        // Billow can't bill until it knows the Business, so the first run starts with its details.
        // Closing them without saving exits Billow. App.xaml sets OnExplicitShutdown so that the
        // setup window closing doesn't end the app before the main window exists.
        if (!BusinessDetailsViewModel.HasSavedBusiness(() => new BillowDbContext()) && !RunSetup())
        {
            Shutdown();
            return;
        }

        MainWindow = new MainWindow();
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        MainWindow.Show();
    }

    /// <summary>Shows Business Details on its own. True if the user saved.</summary>
    private static bool RunSetup()
    {
        var setup = BusinessDetailsWindow.Create(() => new BillowDbContext());

        // It opens as a child of the main window elsewhere; here it has no owner, so give it a
        // taskbar entry and centre it on screen, or it can be lost behind other windows.
        setup.ShowInTaskbar = true;
        setup.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        return setup.ShowDialog() == true;
    }
}
