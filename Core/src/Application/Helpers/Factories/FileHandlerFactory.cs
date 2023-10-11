using System.IO.Abstractions;
using aia_api.Application.FileHandler;
using aia_api.Configuration.Azure;
using aia_api.Services;
using aia_api.src.Services;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Helpers.Factories;

public class FileHandlerFactory : IFileHandlerFactory
{
    private readonly IOptions<Settings> _extensionSettings;
    private readonly IUploadedFileHandler _fileHandlerStreet;
    private readonly IServiceBusService _serviceBusService;
    private readonly AzureService _azureService;
    private readonly IFileSystem _fileSystem;

    public FileHandlerFactory(IOptions<Settings> extensionSettings, AzureService azureService, IFileSystem fileSystem, IServiceBusService serviceBusService)
    {
        _extensionSettings = extensionSettings;
        _azureService = azureService;
        _fileSystem = fileSystem;
        _serviceBusService = serviceBusService; 
        _fileHandlerStreet = BuildFileHandlerStreet();
    }

    private IUploadedFileHandler BuildFileHandlerStreet()
    {
        var fileValidator = new FileValidator(_extensionSettings);
        var zipHandler = new ZipHandler(_extensionSettings, _fileSystem);
        var azureUploader = new UploadHandler(_extensionSettings, _serviceBusService);

        azureUploader.SetClient(_azureService);
        fileValidator.SetNext(zipHandler);
        zipHandler.SetNext(azureUploader);

        return fileValidator;
    }

    public IUploadedFileHandler GetFileHandler()
    {
        return _fileHandlerStreet;
    }
}
