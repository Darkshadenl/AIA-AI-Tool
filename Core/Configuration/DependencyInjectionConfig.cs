using Azure.Storage;
using Azure.Storage.Blobs;
using DotNetEnv;

namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var blobServiceEndpoint = configuration.GetConnectionString("BLOBSERVICEENDPOINT");
        var blobContainerName = configuration.GetConnectionString("BLOBCONTAINERNAME");
        var accountName = configuration.GetConnectionString("ACCOUNTNAME");
        var accountKey = configuration.GetConnectionString("STORAGEACCOUNTKEY");

        var connectionString = new Uri(blobServiceEndpoint + blobContainerName);
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        services.AddSingleton(x => new BlobServiceClient(connectionString, credential));
    }
}
