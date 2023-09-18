using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public interface IFileHandlerFactory
{
    IUploadedFileHandler GetFileHandler();
}

public class FileHandlerFactory : IFileHandlerFactory
{
    private readonly IOptions<Settings> _extensionSettings;
    private readonly IUploadedFileHandler _fileHandlerStreet;

    public FileHandlerFactory(IOptions<Settings> extensionSettings)
    {
        _extensionSettings = extensionSettings;
        _fileHandlerStreet = BuildFilehandlerStreet();
    }

    private IUploadedFileHandler BuildFilehandlerStreet()
    {
        return new ZipHandlerInMemory(_extensionSettings);
    }


    public IUploadedFileHandler GetFileHandler()
    {
        return _fileHandlerStreet;
    }
}
