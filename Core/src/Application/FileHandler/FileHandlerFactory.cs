using aia_api.Application.Azure;
using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public enum FileDataType
{
    MemoryStream,
    FilePath
}

public interface IFileHandlerFactory
{
    IUploadedFileHandler GetFileHandler();
}

public class FileHandlerFactory : IFileHandlerFactory
{
    private readonly IOptions<Settings> _extensionSettings;
    private readonly IUploadedFileHandler _fileHandlerStreet;
    private readonly AzureClient _azureClient;

    public FileHandlerFactory(IOptions<Settings> extensionSettings, AzureClient azureClient)
    {
        _extensionSettings = extensionSettings;
        _azureClient = azureClient;
        _fileHandlerStreet = BuildFileHandlerStreet();
    }

    private IUploadedFileHandler BuildFileHandlerStreet()
    {
        var fileValidator = new FileValidator(_extensionSettings);
        var zipHandler = new ZipHandler(_extensionSettings);
        var azureUploader = new AzureUploadHandler(_extensionSettings);

        azureUploader.setAzureClient(_azureClient);
        fileValidator.SetNext(zipHandler);
        zipHandler.SetNext(azureUploader);

        return fileValidator;
    }

    public IUploadedFileHandler GetFileHandler()
    {
        return _fileHandlerStreet;
    }
}
