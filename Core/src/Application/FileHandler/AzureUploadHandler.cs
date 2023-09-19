using aia_api.Application.Azure;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class AzureUploadHandler : AbstractFileHandler
{
    public AzureUploadHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }

    public void setAzureClient(AzureClient azureClient)
    {

    }

    public override Task Handle(string input, string inputContentType)
    {
        Console.WriteLine("AzureUploadHandler");
        return Task.CompletedTask;
    }

}
