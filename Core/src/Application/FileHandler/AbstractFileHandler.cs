using InterfacesAia;

namespace aia_api.Application.FileHandler;

public abstract class AbstractFileHandler : ICompressedFileHandler
{
    protected ICompressedFileHandler _next;
    protected Dictionary<string, int> _extensionsCount = new();
    protected const int FileSizeInGb = 1 * 1024 * 1024 * 1024;

    public Task<MemoryStream> Handle(MemoryStream input, string extension)
    {
        throw new NotImplementedException();
    }

    public void SetNext(ICompressedFileHandler next)
    {
        _next = next;
    }

    protected bool IsValidFile(MemoryStream input, string inputContentType, string contentType)
    {
        if (inputContentType == contentType && input.Length <= FileSizeInGb) return true;

        if (_next == null)
            throw new Exception("No handler found for this file type.");

        return false;
    }

    protected void CountExtension(string extension)
    {
        if (!_extensionsCount.ContainsKey(extension))
            _extensionsCount[extension] = 1;
        else
            _extensionsCount[extension]++;
    }

    protected bool IsSupportedExtension(string extension)
    {
        return new[] { ".py", ".cs", ".ts", ".js" }.Contains(extension);
    }

    protected void LogExtensionsCount()
    {
        foreach (var (key, value) in _extensionsCount)
            Console.WriteLine($"{key}: {value}");
    }
}
