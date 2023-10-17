using aia_api.Configuration.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace aia_api.Database;

public class PredictionDbContext : DbContext
{
    public DbSet<DbPrediction> Predictions { get; set; }
    public PredictionDbContext(DbContextOptions options) : base(options)
    { }
}
