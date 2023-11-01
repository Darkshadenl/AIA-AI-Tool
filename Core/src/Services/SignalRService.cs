using InterfacesAia;
using InterfacesAia.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace aia_api.Services;

public class SignalRService : ISignalRService
{
    private readonly ILogger<SignalRService> _logger;
    private readonly IServiceBusService _serviceBusService;

    public SignalRService(ILogger<SignalRService> logger, IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }
    
    public async void SendLlmResponseToFrontend(string fileName, string fileExtension, string content)
    {
        HubConnection connection = _serviceBusService.GetConnection();
        if (connection is not { State: HubConnectionState.Connected }) return;

        await connection.InvokeAsync("ReturnLLMResponse", fileName, fileExtension, content);
    }
    
    public async Task InvokeSuccessMessage(string successMessage)
    {
        HubConnection connection = _serviceBusService.GetConnection();
        if (connection.State != HubConnectionState.Connected) return;
        
        await connection.InvokeAsync("UploadSuccess", successMessage);
    }
    
    public async Task InvokeErrorMessage(string errorMessage)
    {
        HubConnection connection = _serviceBusService.GetConnection();
        if (connection.State != HubConnectionState.Connected) return;
        
        await connection.InvokeAsync("ReturnError", errorMessage);
    }
}