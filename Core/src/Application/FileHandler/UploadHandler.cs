using aia_api.Configuration.Azure;
using aia_api.Services;
using aia_api.src.Services;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class UploadHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly IServiceBusService _serviceBusService;
    private AzureService _azureService;
    private const string _uploadSuccessMessage = "File successfully uploaded.";

    public UploadHandler(IOptions<Settings> settings, IServiceBusService serviceBusService) : base(settings)
    {
        _settings = settings;
        _serviceBusService = serviceBusService;
    }

    public void SetClient(AzureService azureService)
    {
        _azureService = azureService;
    }

    public override async Task Handle(string inputPath, string inputContentType)
    {
        try
        {
            await _azureService.Pipeline(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));

            HubConnection connection = _serviceBusService.GetConnection();
            await connection.InvokeAsync("UploadSuccess", _uploadSuccessMessage);

            Console.WriteLine(_uploadSuccessMessage);
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
