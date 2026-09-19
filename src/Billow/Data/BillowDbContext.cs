using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Billow.Data;

public class BillowDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.DataDirectory);
        optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
    }
}
