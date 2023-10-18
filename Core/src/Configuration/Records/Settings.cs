namespace aia_api.Configuration.Records;

public record Settings
{
    public string[] SupportedContentTypes { get; set; }
    public string[] AllowedFiles { get; set; }
    public string TempFolderPath { get; set; }
    public string OutputFolderPath { get; set; }
    public string ServiceBusUrl { get; set; }
    public string SqLiteDbPath { get; set; }
    public string SqLiteDbName { get; set; }
};
