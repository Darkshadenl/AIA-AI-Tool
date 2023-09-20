namespace aia_api.Configuration.Azure;

public record Settings()
{
    public string[] AllowedFiles { get; set; }
};
