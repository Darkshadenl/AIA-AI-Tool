namespace aia_api.Application.Replicate;

public record PredictionInput(
    string Prompt,
    string SystemPrompt,
    int MaxTokens,
    double Temperature,
    double TopP,
    double TopK,
    double FrequencyPenalty,
    double PresencePenalty,
    double RepeatPenalty);
