using Azure.Storage;
using Azure.Storage.Blobs;
namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var blobServiceEndpoint = configuration.GetValue<string>("BLOB_SERVICE_ENDPOINT");
        var blobContainerName = configuration.GetValue<string>("BLOB_CONTAINER_NAME");
        var accountName = configuration.GetValue<string>("ACCOUNT_NAME");
        var accountKey = configuration.GetValue<string>("STORAGE_ACCOUNT_KEY");

        var connectionString = new Uri(blobServiceEndpoint + blobContainerName);
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        services.AddSingleton(new BlobServiceClient(connectionString, credential));
    }
}
