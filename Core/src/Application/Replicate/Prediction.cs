namespace aia_api.Application.Replicate;

public record Prediction(
 string version,
 PredictionInput input,
 string webhook,
 string[]? webhook_events_filter
 );
