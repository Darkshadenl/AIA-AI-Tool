using InterfacesAia;

namespace aia_api.Application.Replicate;

public record ReplicatePredictionDto(
 string version,
 IPredictionInputDto input,
 string webhook
 // string[]? webhook_events_filter
 ) : IReplicatePredictionDto;

public record CodeLLamaPredictionInputDto(
    string prompt,
    double top_p,
    int top_k,
    double temperature,
    int max_tokens,
    double frequency_penalty,
    double presence_penalty,
    double repeat_penalty
    ) : IPredictionInputDto;


public record Llama2PredictionInputDto(
    string prompt,
    double top_p,
    int top_k,
    double temperature,
    string system_prompt,
    int max_new_tokens,
    int min_new_tokens,
    string stop_sequences,
    int seed,
    bool debug) : IPredictionInputDto;

