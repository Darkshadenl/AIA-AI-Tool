using aia_api.Application.Helpers.Factories;
using aia_api.Configuration.Records;
using aia_api.Services;
using aia_api.src.Application;
using aia_api.src.Services;
using Azure.Storage;
using System.IO.Abstractions;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using Azure.Storage.Blobs;
using InterfacesAia;

namespace aia_api.Configuration;

public static class DependencyInjectionConfig
{
    public static void AddProjectConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConfig = configuration.GetSection("AzureBlobStorage");
        var settings = configuration.GetSection("Settings");
        var replicate = configuration.GetSection("ReplicateSettings");
        
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        services.Configure<AzureBlobStorageSettings>(blobConfig);
        services.Configure<Settings>(settings);
        services.Configure<ReplicateSettings>(replicate);
    }

    public static void AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        var aBss = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>();

        if (aBss == null)
            throw new ArgumentNullException(nameof(aBss));

        var connectionString = new Uri(aBss.BlobServiceEndpoint + aBss.BlobContainerName);
        var credential = new StorageSharedKeyCredential(aBss.AccountName, aBss.StorageAccountKey);

        services.AddSingleton<HttpClient>();

        services.AddSingleton(new BlobServiceClient(connectionString, credential));
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IServiceBusService, ServiceBusService>();
        services.AddSingleton<IUploadController, UploadController>();

        services.AddSingleton<AzureService>();
        services.AddSingleton<GitlabService>();
        services.AddSingleton<IFileHandlerFactory, FileHandlerFactory>();
        services.AddSingleton<IFileSystemStorageService, FileSystemStorageService>();
        services.AddSingleton<ReplicateApi>();

    }
}
