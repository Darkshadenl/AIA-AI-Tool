using aia_api.Application.FileHandler.InputTypes;
using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public abstract class AbstractFileHandler : IUploadedFileHandler
{
    protected IUploadedFileHandler Next;
    protected readonly Dictionary<string, int> ExtensionsCount = new();
    private readonly Settings _supportedContentTypes;
    protected FileDataType HandlerType;

    protected int FileSizeInGb = 1 * 1024 * 1024 * 1024;

    protected AbstractFileHandler(IOptions<Settings> extensionSettings)
    {
        _supportedContentTypes = extensionSettings.Value;
    }

    public virtual Task Handle(IInputData input, string inputContentType)
    {
        throw new NotImplementedException();
    }

    public void SetNext(IUploadedFileHandler next)
    {
        Next = next;
    }

    protected FileDataType GetFileDataType(IInputData input)
    {
        if (input is MemoryStreamFileData)
            return FileDataType.MemoryStream;
        if (input is FilePathFileData)
            return FileDataType.FilePath;

        throw new ArgumentException("Invalid type", nameof(input));
    }

    protected bool IsValidFile(MemoryStream input, string inputContentType, string contentType)
    {
        if (inputContentType == contentType && input.Length <= FileSizeInGb) return true;

        if (Next == null)
            throw new Exception("No handler found for this file type.");

        return false;
    }

    protected void CountExtension(string extension)
    {
        if (!ExtensionsCount.ContainsKey(extension))
            ExtensionsCount[extension] = 1;
        else
            ExtensionsCount[extension]++;
    }

    protected bool IsSupportedExtension(string extension)
    {
        var extensions = _supportedContentTypes.AllowedFiles;
        return extensions.Contains(extension);
    }

    protected void LogExtensionsCount()
    {
        foreach (var (key, value) in ExtensionsCount)
            Console.WriteLine($"{key}: {value}");
    }
}
