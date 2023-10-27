using System.IO.Abstractions;
using aia_api.Application.Handlers.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using InterfacesAia.Handlers;
using InterfacesAia.Helpers;
using InterfacesAia.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Helpers.Factories;

public class FileHandlerFactory : IFileHandlerFactory
{
    private readonly IOptions<Settings> _extensionSettings;
    private readonly IUploadedFileHandler _fileHandlerStreet;
    private readonly IFileSystem _fileSystem;
    private readonly IOptions<ReplicateSettings> _replicateSettings;
    private readonly ReplicateApi _replicateApi;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    private readonly CommentChecker _commentChecker;

    public FileHandlerFactory(
        ILogger<FileValidator> fileValidatorLogger,
        ILogger<FileContentsFilter> zipHandlerLogger,
        ILogger<LlmFileUploaderHandler> llmFileUploaderHandlerLogger,
        IOptions<Settings> extensionSettings,
        IFileSystem fileSystem,
        IOptions<ReplicateSettings> replicateSettings,
        ReplicateApi replicateApi,
        IPredictionDatabaseService predictionDatabaseService,
        CommentChecker commentChecker
        )
    {
        _extensionSettings = extensionSettings;
        _fileSystem = fileSystem;
        _replicateSettings = replicateSettings;
        _replicateApi = replicateApi;
        _predictionDatabaseService = predictionDatabaseService;
        _commentChecker = commentChecker;
        _fileHandlerStreet = BuildFileHandlerStreet(fileValidatorLogger, zipHandlerLogger, llmFileUploaderHandlerLogger);
    }

    private IUploadedFileHandler BuildFileHandlerStreet(ILogger<FileValidator> fileValidatorLogger,
                                                        ILogger<FileContentsFilter> zipHandlerLogger,
                                                        ILogger<LlmFileUploaderHandler> llmFileUploaderHandlerLogger)
    {
        var fileValidator = new FileValidator(fileValidatorLogger, _extensionSettings);
        var zipHandler = new FileContentsFilter(zipHandlerLogger, _extensionSettings, _fileSystem, _commentChecker);
        var llm = new LlmFileUploaderHandler(llmFileUploaderHandlerLogger, _extensionSettings, _replicateSettings,
                                             _replicateApi, _fileSystem, _predictionDatabaseService);

        fileValidator.SetNext(zipHandler);
        zipHandler.SetNext(llm);

        return fileValidator;
    }

    public IUploadedFileHandler GetFileHandler()
    {
        return _fileHandlerStreet;
    }
}
