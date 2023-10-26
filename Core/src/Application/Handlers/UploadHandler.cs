using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Handlers;

public class UploadHandler : IUploadController
{
    private readonly ILogger<UploadHandler> _logger;
    private readonly IOptions<Settings> _settings;
    private readonly ISignalRService _signalRService;
    private readonly IFileHandlerFactory _fileHandlerFactory;
    private readonly IFileSystemStorageService _fileSystemStorageService;
    private MemoryStream _memoryStream;

    public UploadHandler(ILogger<UploadHandler> logger, IOptions<Settings> settings, ISignalRService signalRService, 
        IFileHandlerFactory fileHandlerFactory, IFileSystemStorageService fileSystemStorageService)
    {
        _logger = logger;
        _settings = settings;
        _signalRService = signalRService;
        _fileHandlerFactory = fileHandlerFactory;
        _fileSystemStorageService = fileSystemStorageService;
        _memoryStream = new MemoryStream();
    }

    public async void ReceiveFileChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks)
    {
        _logger.LogInformation("Chunk {index} received", index);
        await _memoryStream.WriteAsync(chunk, 0, chunk.Length);

        if (index == totalChunks - 1)
        {
            await _signalRService.InvokeSuccessMessage("File uploaded successfully.");
            ZipHandler(fileName, contentType);
            _memoryStream = new MemoryStream();
        }
    }

    private async void ZipHandler(string fileName, string contentType)
    {
        if (ParamIsEmpty(fileName, "File name is empty.").Result) return;
        if (ParamIsEmpty(contentType, "Content type of file is empty.").Result) return;
        if (_memoryStream.Length <= 0)
        {
            await _signalRService.InvokeErrorMessage("No file received or file is empty.");
            return;
        }

        if (!_settings.Value.SupportedContentTypes.Contains(contentType))
        {
            await _signalRService.InvokeErrorMessage("Invalid file type. Only ZIP files are allowed.");
            return;
        }

        var handlerStreet = _fileHandlerFactory.GetFileHandler();

        try
        {
            var path = await _fileSystemStorageService.StoreInTemp(_memoryStream, fileName);
            var result = await handlerStreet.Handle(path, contentType);
                
            if (result.Success) await _signalRService.InvokeSuccessMessage("File sent to the AI model.");
            else await _signalRService.InvokeErrorMessage(result.ErrorMessage);
        }
        catch (Exception e)
        {
            await _signalRService.InvokeErrorMessage("Something went wrong.");
            _logger.LogCritical("Error: {message}, {stackTrace}", e.Message, e.StackTrace);
        }
    }

    private async Task<bool> ParamIsEmpty(string param, string errorMessage)
    {
        if (string.IsNullOrEmpty(param))
        {
            await _signalRService.InvokeErrorMessage(errorMessage);
            return true;
        }
        return false;
    }
}