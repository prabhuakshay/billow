using Billow.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Billow.Tests;

/// <summary>
/// A real Billow database in a temporary SQLite file, with every migration applied. Deleted on
/// dispose.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"billow-test-{Guid.NewGuid():N}.db");
    private readonly DbContextOptions<BillowDbContext> _options;

    public TestDatabase()
    {
        _options = new DbContextOptionsBuilder<BillowDbContext>()
            .UseSqlite($"Data Source={_path}")
            .Options;

        using var db = Open();
        db.Database.Migrate();
    }

    public BillowDbContext Open() => new(_options);

    public void Dispose()
    {
        // Pooled connections keep the file open, which stops it being deleted on Windows.
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }
}
