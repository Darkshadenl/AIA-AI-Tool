using System.Net;
using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public abstract class AbstractFileHandler : IUploadedFileHandler
{
    protected IUploadedFileHandler Next;
    protected readonly Dictionary<string, int> ExtensionsCount = new();
    private readonly Settings _supportedContentTypes;

    protected AbstractFileHandler(IOptions<Settings> settings)
    {
        _supportedContentTypes = settings.Value;
    }

    public virtual Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        Console.WriteLine($"Input path: {inputPath}\n Input content type: {inputContentType}\n");

        var result = new HandlerResult
        {
            ErrorMessage = "No handler found.",
            Success = false,
            StatusCode = HttpStatusCode.NotImplemented
        };

        return Task.FromResult<IHandlerResult>(result);
    }

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
