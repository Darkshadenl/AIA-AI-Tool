using aia_api.Configuration.Azure;
using aia_api.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class AzureUploadHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private AzureService _azureService;
    private const string UploadSuccessMessage = "File successfully uploaded.";

    public AzureUploadHandler(IOptions<Settings> settings) : base(settings)
    {
        _settings = settings;
    }

    public void setAzureClient(AzureService azureService)
    {
        _azureService = azureService;
    }

    public override async Task Handle(string inputPath, string inputContentType)
    {
        try
        {
            await _azureService.FileSystemPipeline(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));
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
