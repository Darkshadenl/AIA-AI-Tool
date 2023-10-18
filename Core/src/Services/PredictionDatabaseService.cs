using aia_api.Database;
using InterfacesAia;

namespace aia_api.Services;

public class PredictionDatabaseService : IPredictionDatabaseService
{
    private static IServiceScopeFactory _serviceScopeFactory;

    public PredictionDatabaseService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public static async Task<IDbPrediction> CreatePrediction(IDbPrediction prediction)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<PredictionDbContext>();

        await context.AddAsync(prediction);
        await context.SaveChangesAsync();

        return prediction;
    }
}
