namespace aia_api.Configuration.Records;

public record OpenAiSettings
{
    public string ApiToken { get; set; }
    public string ModelName { get; set; }
    public string Message { get; set; }
}
