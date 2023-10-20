using System.Collections;
using System.Net.Mime;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
	public class MainHub : Hub<IMainHub>
	{
        public async Task UploadChunk(string fileName, string contentType, string chunkAsBase64, int index, int totalChunks)
        {
            if (await SendErrorIfEmpty(chunkAsBase64)) return;

            try
            {
                byte[] chunk = Convert.FromBase64String(chunkAsBase64);
                await Clients.Others.UploadChunk(fileName, contentType, chunk, index, totalChunks);
                Console.WriteLine("Chunk {0} of file {1} send to clients", index, fileName);
            }
            catch (FormatException e)
            {
                await Clients.Caller.ReceiveError("The file is not converted to base64 correctly.");
                Console.WriteLine("Error: {0}, stacktrace: {1}", e.Message, e.StackTrace);
            }
        }

        public async Task ReturnLLMResponse(string fileName, string contentType, string fileContent)
        {
            if (await SendErrorIfEmpty(fileContent)) return;

            await Clients.Others.ReturnLLMResponse(fileName, contentType, fileContent);
            Console.WriteLine("File {0} with content type {1} send to clients", fileName, contentType);

        }

        public async Task UploadSuccess(string successMessage)
        {
            await Clients.Others.UploadSuccess(successMessage);
            Console.WriteLine("Success message send: {0}", successMessage);
        }

        public async Task ReturnError(string errorMessage)
        {
            await Clients.Others.ReceiveError(errorMessage);
            Console.WriteLine("Received error: {0}", errorMessage);
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

