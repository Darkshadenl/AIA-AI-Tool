using aia_api.Application.Azure;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class AzureUploadHandler : AbstractFileHandler
{
    private AzureClient _azureClient;
    public AzureUploadHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }

    public void setAzureClient(AzureClient azureClient)
    {
        _azureClient = azureClient;
    }

    public override Task Handle(string inputPath, string outputPath, string inputContentType)
    {
        Console.WriteLine("AzureUploadHandler");
        return Task.CompletedTask;
    }

}
