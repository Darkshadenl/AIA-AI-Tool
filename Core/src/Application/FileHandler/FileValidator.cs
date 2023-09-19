using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class FileValidator : AbstractFileHandler
{
    public FileValidator(IOptions<Settings> extensionSettings) : base(extensionSettings)
    { }


    public override Task<MemoryStream> Handle(IInputData input, string inputContentType)
    {
        return base.Handle(input, inputContentType);
    }
}
