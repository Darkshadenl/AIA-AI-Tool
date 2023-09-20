using aia_api.Configuration.Azure;
using aia_api.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class AzureUploadHandler : AbstractFileHandler
{
    private AzureService _azureService;
    private const string UploadSuccessMessage = "File successfully uploaded.";

    public AzureUploadHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }

    public void setAzureClient(AzureService azureService)
    {
        _azureService = azureService;
    }

    public override async Task Handle(string inputPath, string outputPath, string inputContentType)
    {
        try
        {
            await _azureService.ZipPipeline(outputPath, Path.GetFileName(outputPath));
            Console.WriteLine(UploadSuccessMessage);
        }
        catch (IOException e)
        {
            Console.WriteLine($"Something went wrong while reading or writing: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
            throw;
        }
    }

}
