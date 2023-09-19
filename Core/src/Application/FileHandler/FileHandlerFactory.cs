using aia_api.Application.FileHandler.InputTypes;
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
    IInputData GetInputData(FileDataType type);
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

    public IInputData GetInputData(FileDataType type)
    {
        return type switch
        {
            FileDataType.MemoryStream => new MemoryStreamFileData(),
            FileDataType.FilePath => new FilePathFileData(),
            _ => throw new ArgumentException("Invalid type", nameof(type))
        };
    }
}
