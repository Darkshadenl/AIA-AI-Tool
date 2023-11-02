using System.Collections;
using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
	public class MainHub : Hub<IMainHub>
	{
        private readonly ILogger<MainHub> _logger;

        public MainHub(ILogger<MainHub> logger)
        {
            _logger = logger;
        }
        
        public async Task UploadChunk(string connectionId, string fileName, string contentType, string chunkAsBase64, int index, int totalChunks)
        {
            Console.WriteLine(connectionId);
            if (await SendErrorIfEmpty(chunkAsBase64)) return;

            try
            {
                byte[] chunk = Convert.FromBase64String(chunkAsBase64);
                await Clients.Others.UploadChunk(connectionId, fileName, contentType, chunk, index, totalChunks);
                _logger.LogInformation("Chunk {index} of file {fileName} send to clients", index, fileName);
            }
            catch (FormatException e)
            {
                await Clients.Caller.ReceiveError("The file is not converted to base64 correctly.");
                _logger.LogCritical("Error: {message}, stacktrace: {stackTrace}", e.Message, e.StackTrace);
            }
        }

        public async Task ReturnLLMResponse(string connectionId, string fileName, string contentType, string fileContent, string oldFileContent)
        {
            if (await SendErrorIfEmpty(fileContent) || await SendErrorIfEmpty(oldFileContent)) return;

            await Clients.Client(connectionId).ReturnLLMResponse(connectionId, fileName, contentType, fileContent, oldFileContent);
            _logger.LogInformation("File {fileName} with contentType {contentType} send to client with id {connectionId}", fileName, contentType, connectionId);
        }

        public async Task UploadSuccess(string successMessage)
        {
            await Clients.Others.UploadSuccess(successMessage);
            _logger.LogInformation("Success message send: {message}", successMessage);
        }

        public async Task ReturnError(string errorMessage)
        {
            await Clients.Others.ReceiveError(errorMessage);
            _logger.LogInformation("Received error: {message}", errorMessage);
        }

        public async Task<string> GetConnectionId()
        {
            Console.WriteLine(Context.ConnectionId);
            return Context.ConnectionId;
        }

        private async Task<bool> SendErrorIfEmpty(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                await Clients.Caller.ReceiveError("No file received or file is empty.");
                return true;
            }
            return false;
        }
    }
}

