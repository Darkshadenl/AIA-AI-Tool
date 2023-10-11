using System.Net;
using aia_api.Configuration.Records;
using aia_api.Services;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class UploadHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly IServiceBusService _serviceBusService;
    private AzureService _azureService;
    private const string UploadSuccessMessage = "File successfully uploaded.";

    public UploadHandler(IOptions<Settings> settings, IServiceBusService serviceBusService, AzureService azureService) : base(settings)
    {
        _settings = settings;
        _serviceBusService = serviceBusService;
        _azureService = azureService;
    }

    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        try
        {
            await _azureService.Pipeline(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));

            HubConnection connection = _serviceBusService.GetConnection();
            await connection.InvokeAsync("UploadSuccess", UploadSuccessMessage);

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
        
        if (Next == null)
            return await base.Handle(inputPath, inputContentType);

        return await Next.Handle(inputPath, inputContentType);
    }

}
