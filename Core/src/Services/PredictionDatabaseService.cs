using aia_api.Database;
using InterfacesAia;

namespace aia_api.Services;

public class PredictionDatabaseService : IPredictionDatabaseService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PredictionDatabaseService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<IDbPrediction> CreatePrediction(IDbPrediction prediction)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<PredictionDbContext>();

        await context.AddAsync((DbPrediction) prediction);
        await context.SaveChangesAsync();

        return prediction;
    }
}
