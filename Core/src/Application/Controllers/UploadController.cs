using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Controllers
{

    public class UploadController : IUploadController
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IOptions<Settings> _settings;
        private readonly IServiceBusService _serviceBusService;
        private readonly IFileHandlerFactory _fileHandlerFactory;
        private readonly IFileSystemStorageService _fileSystemStorageService;
        private MemoryStream _memoryStream;

        public UploadController(ILogger<UploadController> logger, IOptions<Settings> settings, IServiceBusService serviceBusService, 
            IFileHandlerFactory fileHandlerFactory, IFileSystemStorageService fileSystemStorageService)
        {
            _logger = logger;
            _settings = settings;
            _serviceBusService = serviceBusService;
            _fileHandlerFactory = fileHandlerFactory;
            _fileSystemStorageService = fileSystemStorageService;
            _memoryStream = new MemoryStream();
        }

        public async void ReceiveFileChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks)
        {
            _logger.LogInformation("Chunk {index} received", index);
            await _memoryStream.WriteAsync(chunk, 0, chunk.Length);
            
            if (index == totalChunks - 1) ZipHandler(fileName, contentType);
        }

        public async void ZipHandler(string fileName, string contentType)
        {
            HubConnection connection = _serviceBusService.GetConnection();

            if (ParamIsEmpty(connection, fileName, "File name is empty.").Result) return;
            if (ParamIsEmpty(connection, contentType, "Content type of file is empty.").Result) return;
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
                var result = await handlerStreet.Handle(path, contentType);
                await InvokeHandleResult(connection, result);
                _memoryStream = new MemoryStream();
            }
            catch (Exception e)
            {
                await InvokeErrorMessage(connection, "Something went wrong.");
                _logger.LogCritical("Error: {message}, {stackTrace}", e.Message, e.StackTrace);
            }
        }

        private async Task InvokeHandleResult(HubConnection connection, IHandlerResult result)
        {
            if (result.Success)
            {
                await connection.InvokeAsync("UploadSuccess", "File successfully uploaded.");
                _logger.LogInformation("File successfully uploaded.");
            }
            else
            {
                await InvokeErrorMessage(connection, result.ErrorMessage);
            }
        }

        private async Task<bool> ParamIsEmpty(HubConnection connection, string param, string errorMessage)
        {
            if (string.IsNullOrEmpty(param))
            {
                await InvokeErrorMessage(connection, errorMessage);
                return true;
            }
            return false;
        }

        private async Task InvokeErrorMessage(HubConnection connection, string errorMessage)
        {
            await connection.InvokeAsync("ReturnError", errorMessage);
            _logger.LogError("Error: {error}", errorMessage);
        }
    }
}

