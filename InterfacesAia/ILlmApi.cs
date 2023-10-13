namespace InterfacesAia;

public interface ILlmApi
{
    Task<HttpResponseMessage> SendPrediction(IReplicatePredictionDto replicateReplicatePredictionDto);
}
