namespace InterfacesAia.Services;

public interface ISignalRService
{
    void SendLlmResponseToFrontend(string connectionId, string fileName, string fileExtension, string content, string inputCode);
    Task InvokeSuccessMessage(string successMessage);
    Task InvokeErrorMessage(string errorMessage);
}