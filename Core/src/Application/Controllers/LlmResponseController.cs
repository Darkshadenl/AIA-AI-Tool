using System.Text;
using aia_api.Database;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;

namespace aia_api.Application.Controllers;

public class LlmResponseController
{
    private readonly ILogger<LlmResponseController> _logger;

    public LlmResponseController(ILogger<LlmResponseController> logger)
    {
        _logger = logger;
    }

    public async void SendLlmResponseToFrontend(DbPrediction dbPrediction, string content, IServiceBusService serviceBusService)
    {
        HubConnection connection = serviceBusService.GetConnection();
        if (connection.State != HubConnectionState.Connected) return;

        await connection.InvokeAsync("ReturnLLMResponse", dbPrediction.FileName, dbPrediction.FileExtension, content);
        _logger.LogInformation("File {fileName} send to clients", dbPrediction.FileName);
    }

    public string CombineTokens(string[] tokens)
    {
        var stringBuilder = new StringBuilder();
        foreach (string token in tokens)
            stringBuilder.Append(token);
        return stringBuilder.ToString().Trim();
    }
}