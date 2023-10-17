﻿using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Controllers
{

    public class UploadController : IUploadController
    {
        private readonly IOptions<Settings> _settings;
        private readonly IServiceBusService _serviceBusService;
        private readonly IFileHandlerFactory _fileHandlerFactory;
        private readonly IFileSystemStorageService _fileSystemStorageService;
        private readonly MemoryStream _memoryStream;

        public UploadController(IOptions<Settings> settings, IServiceBusService serviceBusService, 
            IFileHandlerFactory fileHandlerFactory, IFileSystemStorageService fileSystemStorageService)
        {
            _settings = settings;
            _serviceBusService = serviceBusService;
            _fileHandlerFactory = fileHandlerFactory;
            _fileSystemStorageService = fileSystemStorageService;
            _memoryStream = new MemoryStream();
        }

        public async void ReceiveFileChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks)
        {
            Console.WriteLine("Chunk {0} omgezet naar ByteArray", index);
            await _memoryStream.WriteAsync(chunk, 0, chunk.Length);
            
            if (index == totalChunks - 1) ZipHandler(fileName, contentType);
        }

        public async void ZipHandler(string fileName, string contentType)
        {
            HubConnection connection = _serviceBusService.GetConnection();

            if (ParamIsEmpty(fileName, "File name is empty.").Result) return;
            if (ParamIsEmpty(contentType, "Content type of file is empty.").Result) return;
            if (_memoryStream.Length <= 0)
            {
                await InvokeErrorMessage(connection, "No file received or file is empty.");
                return;
            }

            if (!_settings.Value.SupportedContentTypes.Contains(contentType))
            {
                await InvokeErrorMessage(connection, "Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var handlerStreet = _fileHandlerFactory.GetFileHandler();

            try
            {
                var path = await _fileSystemStorageService.StoreInTemp(_memoryStream, fileName);
                await handlerStreet.Handle(path, contentType);
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

