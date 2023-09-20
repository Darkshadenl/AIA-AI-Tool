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

    public override async Task Handle(string inputPath, string outputPath, string inputContentType)
    {
        try
        {
            await _azureClient.ZipPipeline(outputPath, Path.GetFileName(outputPath));
        }
        catch (IOException e)
        {
            Console.WriteLine($"Something went wrong while reading or writing: {e.Message}");
        }
        catch (Exception e)
        {
            // General exception catch, if none of the above apply
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            throw;
        }
        Console.WriteLine(UploadSuccessMessage);
    }

}
