using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

/// <summary>
/// Checks if file can be handled by any of the handlers.
/// </summary>
public class FileValidator : AbstractFileHandler
{
    private readonly string[] _contentType = { "application/zip" };

    public FileValidator(IOptions<Settings> extensionSettings) : base(extensionSettings)
    { }

    public override async Task Handle(string path, string inputContentType)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The specified file does not exist.");

        var fileInfo = new FileInfo(path);

        if (fileInfo.Length == 0)
            throw new Exception("The file is empty.");

        if (!_contentType.Contains(inputContentType))
            throw new Exception("Invalid file type.");

        await Next.Handle(path, inputContentType);
    }

}
