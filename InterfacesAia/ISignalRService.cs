namespace InterfacesAia;

public interface ISignalRService
{
    void SendLlmResponseToFrontend(string fileName, string fileExtension, string content);
    Task InvokeSuccessMessage(string successMessage);
    Task InvokeErrorMessage(string errorMessage);
}