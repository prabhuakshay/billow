using System.IO;
using Billow.BusinessDetails;
using Microsoft.EntityFrameworkCore;

namespace Billow.Data;

public class BillowDbContext : DbContext
{
    /// <summary>Opens the app's own database in <see cref="AppPaths.DatabasePath"/>.</summary>
    public BillowDbContext()
    {
    }

    /// <summary>Opens the database <paramref name="options"/> points at, e.g. a test database.</summary>
    public BillowDbContext(DbContextOptions<BillowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Business> Businesses => Set<Business>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        Directory.CreateDirectory(AppPaths.DataDirectory);
        optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Business>(business =>
        {
            business.Property(b => b.StateCode).HasMaxLength(2);
            business.Property(b => b.Pin).HasMaxLength(6);
            business.Property(b => b.Pan).HasMaxLength(10);
            business.Property(b => b.RegistrationType).HasConversion<string>();
        });
    }
}
