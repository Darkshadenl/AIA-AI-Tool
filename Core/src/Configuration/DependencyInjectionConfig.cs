using aia_api.Application.Azure;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConfig = configuration.GetSection("AzureBlobStorage");
        services.Configure<AzureBlobStorageSettings>(blobConfig);
    }

    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var c = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>();

        if (c == null)
            throw new ArgumentNullException(nameof(c));

        var blobServiceEndpoint = c.BlobServiceEndpoint;
        var blobContainerName = c.BlobContainerName;
        var accountName = c.AccountName;
        var accountKey = c.StorageAccountKey;

        var connectionString = new Uri(blobServiceEndpoint + blobContainerName);
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        services.AddSingleton(new BlobServiceClient(connectionString, credential));
        services.AddScoped<AzureClient>();
    }
}
