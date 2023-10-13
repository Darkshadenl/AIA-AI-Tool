namespace InterfacesAia;

public interface IPredictionInputDto
{
    string prompt { get; init; }
    double top_p { get; init; }
    int top_k { get; init; }
    double temperature { get; init; }
}
