using System.Collections;
using System.Net.Mime;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
	public class MainHub : Hub<IMainHub>
	{
        public async Task UploadChunk(string fileName, string contentType, string chunkAsBase64, int index, int totalChunks)
        {
            if (string.IsNullOrEmpty(chunkAsBase64))
            {
                await Clients.Caller.ReceiveError("No file received or file is empty.");
                return;
            }

            try
            {
                byte[] chunk = Convert.FromBase64String(chunkAsBase64);
                await Clients.Others.UploadChunk(fileName, contentType, chunk, index, totalChunks);
            }
            catch (FormatException e)
            {
                await Clients.Caller.ReceiveError("The file is not converted to base64 correctly.");
                Console.WriteLine("Error: {0}, stacktrace: {1}", e.Message, e.StackTrace);
            }
        }

        public async Task UploadSuccess(string successMessage) => await Clients.Others.UploadSuccess(successMessage);
        public async Task ReturnError(string errorMessage) => await Clients.Others.ReceiveError(errorMessage);
    }
}

