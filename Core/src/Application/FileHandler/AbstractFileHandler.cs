using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public abstract class AbstractFileHandler : IUploadedFileHandler
{
    protected IUploadedFileHandler Next;
    protected readonly Dictionary<string, int> ExtensionsCount = new();
    private readonly Settings _supportedContentTypes;

    protected AbstractFileHandler(IOptions<Settings> extensionSettings)
    {
        _supportedContentTypes = extensionSettings.Value;
    }

    public abstract Task Handle(string input, string inputContentType);

    public void SetNext(IUploadedFileHandler next)
    {
        Next = next;
    }

    protected bool IsValidFile(string inputContentType, string contentType)
    {
        if (inputContentType == contentType) return true;

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
