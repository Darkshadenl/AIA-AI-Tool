namespace InterfacesAia;

public interface IPredictionDatabaseService
{
    static abstract Task<IDbPrediction> CreatePrediction(IDbPrediction prediction);
}
