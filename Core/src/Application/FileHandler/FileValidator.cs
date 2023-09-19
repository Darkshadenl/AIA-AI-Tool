using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

/// <summary>
/// Checks if file can be handled by any of the handlers.
/// </summary>
public class FileValidator : AbstractFileHandler
{
    private readonly string[] _contentType = new string[] { "application/zip" };

    public FileValidator(IOptions<Settings> extensionSettings) : base(extensionSettings)
    { }

    public override Task Handle(IInputData input, string inputContentType)
    {
        GetFileDataType(input);
        compatibleHandlerExists(inputContentType);

        throw new Exception("No handler found for this file type.");
    }

    private bool compatibleHandlerExists(string inputContentType)
    {
        if (inputContentType == contentType && input.Length <= FileSizeInGb) return true;

        if (Next == null)
            throw new Exception("No handler found for this file type.");

        return false;
    }


}
