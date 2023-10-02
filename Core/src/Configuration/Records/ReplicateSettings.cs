namespace aia_api.Configuration.Records;

public record ReplicateSettings
{
    public string ApiToken { get; set; }
    public string PredictionsUrl { get; set; }
}
