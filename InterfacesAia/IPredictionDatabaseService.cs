namespace InterfacesAia;

public interface IPredictionDatabaseService
{
    Task<IDbPrediction> CreatePrediction(IDbPrediction prediction);
}
