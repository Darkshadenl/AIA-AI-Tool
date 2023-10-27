namespace InterfacesAia.Database;

public interface IReplicatePredictionDto
{
    string version { get; init; }
    IPredictionInputDto input { get; init; }
    string webhook { get; init; }
}
