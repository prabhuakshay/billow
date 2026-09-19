using System.IO;
using Billow.BusinessDetails;
using Billow.Customers;
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

    public DbSet<Customer> Customers => Set<Customer>();

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
            business.Property(b => b.Gstin).HasMaxLength(15);
            business.Property(b => b.Pan).HasMaxLength(10);
            business.Property(b => b.Ifsc).HasMaxLength(11);
            business.Property(b => b.RegistrationType).HasConversion<string>();
            business.HasMany(b => b.AdditionalRegistrations).WithOne().HasForeignKey(r => r.BusinessId);
        });

        modelBuilder.Entity<AdditionalRegistration>().ToTable("AdditionalRegistrations");

        modelBuilder.Entity<Customer>(customer =>
        {
            customer.Property(c => c.StateCode).HasMaxLength(2);
            customer.Property(c => c.Pin).HasMaxLength(6);
            customer.Property(c => c.Gstin).HasMaxLength(15);

            // A GSTIN belongs to one Customer only. SQLite lets any number of rows share a null.
            customer.HasIndex(c => c.Gstin).IsUnique();
        });
    }
}
