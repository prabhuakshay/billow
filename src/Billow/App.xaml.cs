using System.Windows;
using Billow.Data;
using Microsoft.EntityFrameworkCore;

namespace Billow;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Create the database on first run, and apply any schema changes after an update.
        using (var db = new BillowDbContext())
        {
            db.Database.Migrate();
        }

        base.OnStartup(e);
    }
}
