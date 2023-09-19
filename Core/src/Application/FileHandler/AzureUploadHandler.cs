using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class AzureUploadHandler : AbstractFileHandler
{
    public AzureUploadHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }
}
