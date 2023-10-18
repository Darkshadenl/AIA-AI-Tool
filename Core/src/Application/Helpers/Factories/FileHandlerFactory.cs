using System.IO.Abstractions;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Services;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Helpers.Factories;

public class FileHandlerFactory : IFileHandlerFactory
{
    private readonly IOptions<Settings> _extensionSettings;
    private readonly IUploadedFileHandler _fileHandlerStreet;
    private readonly IFileSystem _fileSystem;
    private readonly IOptions<ReplicateSettings> _replicateSettings;
    private readonly ReplicateApi _replicateApi;
    private readonly PredictionDbContext _dbContext;

    public FileHandlerFactory(
        IOptions<Settings> extensionSettings,
        IFileSystem fileSystem,
        IOptions<ReplicateSettings> replicateSettings,
        IFileSystemStorageService fileSystemStorageService,
        ReplicateApi replicateApi,
        PredictionDbContext dbContext
        )
    {
        _extensionSettings = extensionSettings;
        _fileSystem = fileSystem;
        _replicateSettings = replicateSettings;
        _replicateApi = replicateApi;
        _dbContext = dbContext;
        _fileHandlerStreet = BuildFileHandlerStreet();
    }

    private IUploadedFileHandler BuildFileHandlerStreet()
    {
        var fileValidator = new FileValidator(_extensionSettings);
        var zipHandler = new ZipHandler(_extensionSettings, _fileSystem);
        var llm = new LlmFileUploaderHandler(_extensionSettings, _replicateSettings, _replicateApi, _fileSystem, _dbContext);

        fileValidator.SetNext(zipHandler);
        zipHandler.SetNext(llm);

        return fileValidator;
    }

    public IUploadedFileHandler GetFileHandler()
    {
        return _fileHandlerStreet;
    }
}
