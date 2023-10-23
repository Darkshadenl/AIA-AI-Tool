using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using aia_api.Application.Helpers;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Services;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class LlmFileUploaderHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly ReplicateApi _replicateApi;
    private readonly IFileSystem _fileSystem;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    private readonly ReplicateSettings _replicateSettings;
    private readonly List<string> _errors = new();

    public LlmFileUploaderHandler(
        ILogger<LlmFileUploaderHandler> logger,
        IOptions<Settings> settings,
        IOptions<ReplicateSettings> replicateSettings,
        ILlmApi replicateApi,
        IFileSystem fileSystem,
        IPredictionDatabaseService predictionDatabaseService
        ) : base(logger, settings)
    {
        _settings = settings;
        _replicateApi = (ReplicateApi) replicateApi;
        _fileSystem = fileSystem;
        _predictionDatabaseService = predictionDatabaseService;
        _replicateSettings = replicateSettings.Value;
    }

    /// <summary>
    /// This handle method expects a zip-file at the outputPath with the name of the file in the inputPath.
    /// If it does not exist, it will throw an exception.
    /// </summary>
    /// <throws>FileNotFoundException  if zip-file cannot be found</throws>
    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        var fileName = _fileSystem.Path.GetFileName(inputPath);
        var outputFilePath = _fileSystem.Path.Combine(_settings.Value.OutputFolderPath, fileName);
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
        var customPrompt = _replicateSettings.Prompt.Replace("${code}", inputCode);

        var dbPrediction = new DbPrediction
        {
            FileExtension = fileExtension,
            FileName = file.FullName,
            Prompt = customPrompt,
            InputCode = inputCode
        };

        return await _predictionDatabaseService.CreatePrediction(dbPrediction);
    }

    private async Task ProcessFiles(ZipArchive zipArchive)
    {
        // var maxNumber = 2;
        // var number = 0;

        foreach (var file in zipArchive.Entries)
        {
            // if (number++ > maxNumber) break; 

            var fileExtension = _fileSystem.Path.GetExtension(file.FullName);
            if (string.IsNullOrEmpty(fileExtension)) continue;
            await ProcessFile(file);
        }
    }

    private async Task ProcessFile(ZipArchiveEntry file)
    {
        var dbPrediction = await SavePredictionToDatabase(file);
        var webHookWithId = _replicateSettings.WebhookUrl.Replace("${dbPredictionId}", dbPrediction.Id.ToString());
        var prediction = _replicateApi.CreateCodeLlamaPrediction(dbPrediction, webHookWithId);

        if (EnvHelper.ReplicateEnabled())
        {
            var response = await _replicateApi.SendPrediction(prediction);
            if (!response.IsSuccessStatusCode)
                _errors.Add($"File: {file.FullName}, Error: {response.ReasonPhrase}");
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
