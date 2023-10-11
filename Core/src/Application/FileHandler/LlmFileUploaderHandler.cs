using System.IO.Abstractions;
using System.IO.Compression;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class LlmFileUploaderHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly ReplicateApi _replicateApi;
    private readonly IFileSystem _fileSystem;
    private readonly ReplicateSettings _replicateSettings;

    public LlmFileUploaderHandler(IOptions<Settings> settings,
        IOptions<ReplicateSettings> replicateSettings, ReplicateApi replicateApi, IFileSystem fileSystem) : base(settings)
    {
        _settings = settings;
        _replicateApi = replicateApi;
        _fileSystem = fileSystem;
        _replicateSettings = replicateSettings.Value;
    }

    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        // retrieve files of upload
        var fileName = _fileSystem.Path.GetFileName(inputPath);
        var outputFilePath = _fileSystem.Path.Combine(_settings.Value.OutputFolderPath, fileName);

        await using var fileStream = _fileSystem.FileStream.New(outputFilePath, FileMode.Open, FileAccess.Read);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Read);

        foreach (var file in zipArchive.Entries)
        {
            var fileExtension = _fileSystem.Path.GetExtension(file.FullName);
            if (string.IsNullOrEmpty(fileExtension)) continue;

            // make a prediction for every file
            // make a custom prompt for every file


            // save basedata to the database for id retrieval
            var dbPrediction = new DbPrediction
            {
                FileExtension = fileExtension,
                FileName = file.FullName,

            };

            // use sqlite id for every file

            // send the prediction replicate
        }

        var prediction = new Prediction(
            version: _replicateSettings.ModelVersion,
            input: new PredictionInput(
                prompt: _replicateSettings.Prompt,
                // SystemPrompt: _replicateSettings.SystemPrompt,
                max_tokens: 500,
                temperature: 0.8,
                top_p: 0.95,
                top_k: 10,
                frequency_penalty: 0,
                presence_penalty: 0,
                repeat_penalty: 1.1
            ),
            webhook: _replicateSettings.WebhookUrl
            // webhook_events_filter: _replicateSettings.WebhookFilters
        );

        var response = await _replicateApi.RunPrediction(prediction);

        return new HandlerResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            ErrorMessage = response.ReasonPhrase ?? string.Empty
        };
    }

}
