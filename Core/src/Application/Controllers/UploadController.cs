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
        private readonly IServiceBusService _serviceBusService;
        private readonly IFileHandlerFactory _fileHandlerFactory;
        private readonly IStorageService _storageService;

        public UploadController(IServiceBusService serviceBusService, IFileHandlerFactory fileHandlerFactory, IStorageService storageService)
		{
            _serviceBusService = serviceBusService;
            _fileHandlerFactory = fileHandlerFactory;
            _storageService = storageService;
        }

        public async void ZipHandler(string fileName, string contentType, string fileBase64)
        {
            HubConnection connection = _serviceBusService.GetConnection();

            if (ParamIsEmpty(fileName, "File name is empty.").Result) return;
            if (ParamIsEmpty(contentType, "Content type of file is empty.").Result) return;
            if (ParamIsEmpty(fileBase64, "No file received or file is empty.").Result) return;

            if (!_supportedContentTypes.Contains(contentType))
            {
                await InvokeErrorMessage(connection, "Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var handlerStreet = _fileHandlerFactory.GetFileHandler();

            try
            {
                byte[] fileByteArray = Convert.FromBase64String(fileBase64);
                Stream inputStream = new MemoryStream(fileByteArray);
                var path = await _storageService.StoreInTemp(inputStream, fileName);
                await handlerStreet.Handle(path, contentType, inputStream);
            }
            catch (FormatException)
            {
                await InvokeErrorMessage(connection, "File is could not be parsed to Base64.");
            }
            catch (Exception e)
            {
                await InvokeErrorMessage(connection, "Something went wrong.");
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
            }
        }

        private async Task<bool> ParamIsEmpty(string param, string errorMessage)
        {
            HubConnection connection = _serviceBusService.GetConnection();
            if (string.IsNullOrEmpty(param))
            {
                await InvokeErrorMessage(connection, errorMessage);
                return true;
            }
            return false;
        }

        private static async Task InvokeErrorMessage(HubConnection connection, string errorMessage)
        {
            await connection.InvokeAsync("ReturnError", errorMessage);
            Console.WriteLine(errorMessage);
        }
    }
}

