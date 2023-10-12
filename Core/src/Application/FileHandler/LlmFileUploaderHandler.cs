using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
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
    private readonly PredictionDbContext _dbContext;
    private readonly ReplicateSettings _replicateSettings;

    public LlmFileUploaderHandler(
        IOptions<Settings> settings,
        IOptions<ReplicateSettings> replicateSettings,
        ReplicateApi replicateApi,
        IFileSystem fileSystem,
        PredictionDbContext dbContext
        ) : base(settings)
    {
        _settings = settings;
        _replicateApi = replicateApi;
        _fileSystem = fileSystem;
        _dbContext = dbContext;
        _replicateSettings = replicateSettings.Value;
    }

    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        // retrieve files of upload
        var fileName = _fileSystem.Path.GetFileName(inputPath);
        var outputFilePath = _fileSystem.Path.Combine(_settings.Value.OutputFolderPath, fileName);

        await using var fileStream = _fileSystem.FileStream.New(outputFilePath, FileMode.Open, FileAccess.Read);
        using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Read);

        var maxNumber = 2;
        var number = 0;

        var responses = new List<HttpResponseMessage>();

        foreach (var file in zipArchive.Entries)
        {
            if (number++ > maxNumber) break;    // TODO remove if you want to analyse all files

            var fileExtension = _fileSystem.Path.GetExtension(file.FullName);
            if (string.IsNullOrEmpty(fileExtension)) continue;

            // determine if file has a comment
            var dbPrediction = await SavePredictionToDatabase(file);
            var webHookWithId = _replicateSettings.WebhookUrl.Replace("${dbPredictionId}",  dbPrediction.Id.ToString());
            var prediction = _replicateApi.CreatePrediction(dbPrediction, webHookWithId);

            // send the prediction replicate
            responses.Add(await _replicateApi.SendPrediction(prediction));
        }

        foreach (var response in responses)
        {
            if (response.IsSuccessStatusCode) continue;

            Console.WriteLine(response);
            return new HandlerResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                ErrorMessage = response.ReasonPhrase ?? string.Empty
            };
        }

        return new HandlerResult
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            ErrorMessage = "OK"
        };
    }

    private async Task<DbPrediction> SavePredictionToDatabase(ZipArchiveEntry file)
    {
        var fileExtension = _fileSystem.Path.GetExtension(file.FullName);
        var customPrompt = _replicateSettings.Prompt.Replace("${code}", "code here");

        var dbPrediction = new DbPrediction
        {
            FileExtension = fileExtension,
            FileName = file.FullName,
            Prompt = customPrompt
        };

        await _dbContext.AddAsync(dbPrediction);
        await _dbContext.SaveChangesAsync();

        return dbPrediction;
    }

}
