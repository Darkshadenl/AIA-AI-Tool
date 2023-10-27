using InterfacesAia.Database;

namespace InterfacesAia.Services;

public interface IPredictionDatabaseService
{
    Task<IDbPrediction> CreatePrediction(IDbPrediction prediction);
    IDbPrediction GetPrediction(int predictionId);
    void UpdatePrediction(IDbPrediction dbPrediction, string responseText);
}
