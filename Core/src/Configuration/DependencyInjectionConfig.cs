using System.IO.Abstractions;
using aia_api.Application.FileHandler;
using aia_api.Application.Helpers.Factories;
using aia_api.Configuration.Azure;
using aia_api.Services;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConfig = configuration.GetSection("AzureBlobStorage");
        var settings = configuration.GetSection("Settings");

        services.Configure<AzureBlobStorageSettings>(blobConfig);
        services.Configure<Settings>(settings);
    }

    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var aBss = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>();

        if (aBss == null)
            throw new ArgumentNullException(nameof(aBss));

        var connectionString = new Uri(aBss.BlobServiceEndpoint + aBss.BlobContainerName);
        var credential = new StorageSharedKeyCredential(aBss.AccountName, aBss.StorageAccountKey);

        services.AddSingleton(new BlobServiceClient(connectionString, credential));
        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddScoped<AzureService>();
        services.AddScoped<HttpClient>();
        services.AddScoped<GitlabService>();
        services.AddScoped<IFileHandlerFactory, FileHandlerFactory>();
        services.AddScoped<IStorageService, StorageService>();

    }
}
