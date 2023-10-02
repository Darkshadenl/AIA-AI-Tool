namespace aia_api.Configuration.Records;

public record ReplicateSettings
{
    public string ApiToken { get; set; }
    public string PredictionsUrl { get; set; }
    public string ModelVersion { get; set; }
    public string ModelName { get; set; }
    public string WebhookUrl { get; set; }
    public string[] WebhookFilters { get; set; }
    public string Prompt { get; set; }
    public string SystemPrompt { get; set; }
}
