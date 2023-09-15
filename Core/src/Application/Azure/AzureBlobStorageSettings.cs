namespace aia_api.Application.Azure;

public record AzureBlobStorageSettings
{
    public string StorageAccountKey { get; set; }
    public string AccountName { get; set; }
    public string BlobServiceEndpoint { get; set; }
    public string BlobContainerName { get; set; }
}
