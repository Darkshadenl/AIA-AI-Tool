using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using aia_api.Application.Helpers;
using aia_api.Application.OpenAi;
using aia_api.Configuration.Records;
using aia_api.Database;
using Azure.AI.OpenAI;
using InterfacesAia.Database;
using InterfacesAia.Handlers;
using InterfacesAia.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Handlers.FileHandler;

/// <summary>
/// Upload files to OpenAi for processing
/// </summary>
public class LlmFileUploaderHandler : AbstractFileHandler
{
    private readonly ILogger<LlmFileUploaderHandler> _logger;
    private readonly Settings _settings;
    private readonly OpenAiSettings _openAiSettings;
    private readonly OpenAiApi _openAiApi;
    private readonly IFileSystem _fileSystem;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    private List<string> _errors;
    private string _clientConnectionId;

    public LlmFileUploaderHandler(
        ILogger<LlmFileUploaderHandler> logger,
        IOptions<Settings> settings,
        IOptions<OpenAiSettings> openAiSettings,
        OpenAiApi openAiApi,
        IFileSystem fileSystem,
        IPredictionDatabaseService predictionDatabaseService
        ) : base(logger, settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _openAiSettings = openAiSettings.Value;
        _openAiApi = openAiApi;
        _fileSystem = fileSystem;
        _predictionDatabaseService = predictionDatabaseService;
    }

    /// <summary>
    /// This handle method expects a zip-file at the outputPath with the name of the file in the inputPath.
    /// If it does not exist, it will throw an exception.
    /// </summary>
    /// <throws>FileNotFoundException if zip-file cannot be found</throws>
    public override async Task<IHandlerResult> Handle(string clientConnectionId, string inputPath, string inputContentType)
    {
        _errors = new();
        _clientConnectionId = clientConnectionId;
        var fileName = _fileSystem.Path.GetFileName(inputPath);
        var outputFilePath = _fileSystem.Path.Combine(_settings.TempFolderPath + "Output/", fileName);
        var zipArchive = GetZipArchive(outputFilePath);
        
        await ProcessFiles(zipArchive);
        return CreateHandlerResult();
    }

    private ZipArchive GetZipArchive(string outputFilePath)
    {
        var fileStream = _fileSystem.FileStream.New(outputFilePath, FileMode.Open, FileAccess.Read);
        return new ZipArchive(fileStream, ZipArchiveMode.Read);
    }

    private async Task<IDbPrediction> SavePredictionToDatabase(ZipArchiveEntry file)
    {
        var fileExtension = _fileSystem.Path.GetExtension(file.FullName);

        using var reader = new StreamReader(file.Open());
        string inputCode = await reader.ReadToEndAsync();
        var customPrompt = _openAiSettings.Prompt.Replace("${code}", inputCode);

        var dbPrediction = new DbPrediction
        {
            ClientConnectionId = _clientConnectionId,
            ModelName = _openAiSettings.ModelName,
            FileExtension = fileExtension,
            FileName = file.FullName,
            SystemPrompt = _openAiSettings.SystemPrompt,
            Prompt = customPrompt,
            InputCode = inputCode
        };

        return await _predictionDatabaseService.CreatePrediction(dbPrediction);
    }

    private async Task ProcessFiles(ZipArchive zipArchive)
    {
        var number = 0;

        foreach (var file in zipArchive.Entries)
        {
            number++;
            _logger.LogInformation("Uploading {number} of {Count} files... \nFilename: {FullName}", number, zipArchive.Entries.Count, file.FullName);
            await ProcessFile(file);
        }
    }

    private async Task ProcessFile(ZipArchiveEntry file)
    {
        var dbPrediction = await SavePredictionToDatabase(file);

        if (EnvHelper.OpenAiEnabled())
        {
            var time = DateTime.Now;
            var openAiResponse = await _openAiApi.SendOpenAiCompletion(dbPrediction);
            var newTime = DateTime.Now;
            _logger.LogDebug($"Duration: {newTime - time} - Finish reason: {openAiResponse.FinishReason}");
            
            CheckIfErrors(openAiResponse, file);
            if (_errors.Count > 0) return;
            
            _openAiApi.ProcessApiResponse(openAiResponse, dbPrediction);
            _logger.LogInformation("Llm response for {fileName} with id {id} was successfully processed", dbPrediction.FileName, dbPrediction.Id);
        }
    }

    private void CheckIfErrors(ChatChoice openAiResponse, ZipArchiveEntry file)
    {
        if (openAiResponse.Message.Content.Length <= 0) 
            _errors.Add($"File: {file.FullName}, Error: No content received from LLM.");
        if (openAiResponse.FinishReason == CompletionsFinishReason.TokenLimitReached)
            _errors.Add($"File {file.FullName}, Error: Token limit reached for message.");
        if (openAiResponse.FinishReason == CompletionsFinishReason.ContentFiltered)
            _errors.Add($"File {file.FullName}, Error: Potentially sensitive content found and filtered from the LLM result.");
        if (openAiResponse.FinishReason == null)
            _errors.Add($"File {file.FullName}, Error: LLM is still processing the request.");
    }

    private HandlerResult CreateHandlerResult()
    {
        if (_errors.Count > 0)
        {
            return new HandlerResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessage = string.Join("; ", _errors)
            };
        }

        return new HandlerResult
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            ErrorMessage = "OK"
        };
    }

}
