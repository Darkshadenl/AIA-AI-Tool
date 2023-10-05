using System;
using InterfacesAia;
using aia_api.Application.Helpers.Factories;
using aia_api.Services;
using aia_api.src.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace aia_api.src.Application
{

    public class UploadController : IUploadController
    {
        private readonly string[] _supportedContentTypes = { "application/zip" };
        private readonly IFileHandlerFactory _fileHandlerFactory;
        private readonly IStorageService _storageService;

        public UploadController(IFileHandlerFactory fileHandlerFactory, IStorageService storageService)
		{
			_fileHandlerFactory = fileHandlerFactory;
            _storageService = storageService;
        }

        public async void ZipHandler(string fileName, string contentType, string file)
        {
            HubConnection connection = ServiceBusService.GetConnection();

            if (string.IsNullOrEmpty(file))
            {
                await connection.InvokeAsync("ReturnError", "No file received or file is empty.");
                Console.WriteLine("No file received or file is empty.");
                return;
            }

            if (!_supportedContentTypes.Contains(contentType))
            {
                await connection.InvokeAsync("ReturnError", "Invalid file type. Only ZIP files are allowed.");
                Console.WriteLine("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var handlerStreet = _fileHandlerFactory.GetFileHandler();

            try
            {
                byte[] fileByteArray = Convert.FromBase64String(file);
                Stream inputStream = new MemoryStream(fileByteArray);
                var path = await _storageService.StoreInTemp(inputStream, fileName);
                await handlerStreet.Handle(path, contentType, inputStream);
            }
            catch (Exception e)
            {
                throw new Exception($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
            }
        }
    }
}

