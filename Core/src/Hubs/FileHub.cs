using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace aia_api.src.Hubs
{
	public class FileHub : Hub
	{
        public async Task SendData(string encodedFile)
        {
            Console.WriteLine("File: " + encodedFile);

            string fileAsBase64 = encodedFile.Split(',')[1];

            await Clients.All.SendAsync("ReceiveData", fileAsBase64);
        }
    }
}

