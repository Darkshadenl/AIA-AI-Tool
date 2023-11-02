using System;
namespace SignalR
{
    public interface IMainHub
    {
        public Task UploadChunk(string connectionId, string fileName, string contentType, byte[] chunk, int index, int totalChunks);
        public Task ReturnLLMResponse(string connectionId, string fileName, string contentType, string fileContent, string oldFileContent);
        public Task UploadSuccess(string successMessage);
        public Task ReceiveError(string errorMessage);
        public Task<string> GetConnectionId();
    }
}