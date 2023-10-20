using System;
namespace SignalR
{
    public interface IMainHub
    {
        public Task UploadChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks);
        public Task ReturnLLMResponse(string fileName, string contentType, string fileContent);
        public Task UploadSuccess(string successMessage);
        public Task ReceiveError(string errorMessage);
    }
}