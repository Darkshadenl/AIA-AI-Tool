using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using aia_api.Application.Helpers;
using aia_api.Application.OpenAi;
using aia_api.Configuration.Records;
using aia_api.Database;
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
    private readonly OpenAiApi _openAiApi;
    private readonly IFileSystem _fileSystem;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    private readonly OpenAiSettings _openAiSettings;
    private List<string> _errors;

    public LlmFileUploaderHandler(
        ILogger<LlmFileUploaderHandler> logger,
        IOptions<Settings> settings,
        IOptions<OpenAiSettings> openAiSettings,
        OpenAiApi openAiApi,
        ISignalRService signalRService,
        CommentManipulationHelper commentManipulationHelper,
        IFileSystem fileSystem,
        IPredictionDatabaseService predictionDatabaseService
        ) : base(logger, settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _openAiApi = openAiApi;
        _fileSystem = fileSystem;
        _predictionDatabaseService = predictionDatabaseService;
        _openAiSettings = openAiSettings.Value;
    }

    /// <summary>
    /// This handle method expects a zip-file at the outputPath with the name of the file in the inputPath.
    /// If it does not exist, it will throw an exception.
    /// </summary>
    /// <throws>FileNotFoundException if zip-file cannot be found</throws>
    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        _errors = new();
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

            if (openAiResponse.FinishReason != null)
            {
                Console.WriteLine(newTime - time);
                Console.WriteLine(openAiResponse.Message.Content);

                _openAiApi.ProcessApiResponse(openAiResponse, dbPrediction);
                _logger.LogInformation("Llm response for {fileName} with id {id} was successfully processed", dbPrediction.Id, dbPrediction.FileName);
            }
        }
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
