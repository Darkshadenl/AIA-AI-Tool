namespace aia_api.Configuration.Records;

public record Settings
{
    public string[] AllowedFiles { get; set; }
    public string TempFolderPath { get; set; }
    public string OutputFolderPath { get; set; }
};
