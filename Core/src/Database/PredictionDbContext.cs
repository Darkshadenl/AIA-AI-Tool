using aia_api.Configuration.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace aia_api.Database;

public class PredictionDbContext : DbContext
{
    public DbSet<DbPrediction> Predictions { get; set; }
    public string DbPath { get; }

    public PredictionDbContext(IOptions<Settings> settings, DbContextOptions<PredictionDbContext> options) : base(options)
    {
        DbPath = settings.Value.SqLiteDbPath + settings.Value.SqLiteDbName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
