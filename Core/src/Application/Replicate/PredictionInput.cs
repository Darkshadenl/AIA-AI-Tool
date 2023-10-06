namespace aia_api.Application.Replicate;

public record PredictionInput(
    string prompt,
    // string SystemPrompt,
    int max_tokens,
    double temperature,
    double top_p,
    double top_k,
    double frequency_penalty,
    double presence_penalty,
    double repeat_penalty
    );
