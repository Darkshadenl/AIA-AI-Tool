namespace InterfacesAia;

public interface IReplicatePredictionDto
{
    string version { get; init; }
    IPredictionInputDto input { get; init; }
    string webhook { get; init; }
}