namespace InterfacesAia.Services;

public interface ISignalRService
{
    void SendLlmResponseToFrontend(string connectionId, string fileName, string fileExtension, string content, string inputCode);
    Task InvokeProgressInformationMessage(string connectionId, string progressInformationMessage);
    Task InvokeErrorMessage(string connectionId, string errorMessage);
}