using System.Net;
using aia_api.Configuration.Records;
using aia_api.Services;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class UploadHandler : AbstractFileHandler
{
    private readonly ILogger<UploadHandler> _logger;
    private readonly IOptions<Settings> _settings;
    private readonly AzureService _azureService;

    public UploadHandler(ILogger<UploadHandler> logger, IOptions<Settings> settings, AzureService azureService) : base(logger, settings)
    {
        _logger = logger;
        _settings = settings;
        _azureService = azureService;
    }

    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        try
        {
            await _azureService.Pipeline(_settings.Value.TempFolderPath + "Output/", Path.GetFileName(inputPath));
            _logger.LogInformation("File successfully uploaded to Azure.");
        }
        catch (IOException e)
        {
            _logger.LogCritical("Something went wrong while reading or writing: {message}, {stackTrace}", e.Message, e.StackTrace);
        }
        catch (Exception e)
        {
            _logger.LogCritical("An unexpected error occurred: {message}, {stackTrace}", e.Message, e.StackTrace);
            throw;
        }

        if (Next == null)
            return await base.Handle(inputPath, inputContentType);

        return await Next.Handle(inputPath, inputContentType);
    }

}
