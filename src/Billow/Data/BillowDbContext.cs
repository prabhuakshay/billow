using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Billow.Data;

public class BillowDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (options.IsConfigured)
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.DataDirectory);
        options.UseSqlite($"Data Source={AppPaths.DatabasePath}");
    }
}
