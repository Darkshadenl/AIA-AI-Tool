using aia_api.Application.Azure;
using aia_api.Application.FileHandler;
using aia_api.Configuration.Azure;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConfig = configuration.GetSection("AzureBlobStorage");
        services.Configure<AzureBlobStorageSettings>(blobConfig);
        var settings = configuration.GetSection("Settings");
        services.Configure<Settings>(settings);
    }

    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var azureBlobStorageSettings = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>();

        if (azureBlobStorageSettings == null)
            throw new ArgumentNullException(nameof(azureBlobStorageSettings));

        var blobServiceEndpoint = azureBlobStorageSettings.BlobServiceEndpoint;
        var blobContainerName = azureBlobStorageSettings.BlobContainerName;
        var accountName = azureBlobStorageSettings.AccountName;
        var accountKey = azureBlobStorageSettings.StorageAccountKey;

        var connectionString = new Uri(blobServiceEndpoint + blobContainerName);
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        services.AddSingleton(new BlobServiceClient(connectionString, credential));
        services.AddScoped<AzureClient>();
        services.AddScoped<IFileHandlerFactory, FileHandlerFactory>();
    }
}
