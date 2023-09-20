using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Services;

public interface IStorageService
{
    /// <summary>
    /// Stores the content of a response in the temp folder.
    /// Configure this folder in appsettings.json
    /// </summary>
    /// <param name="response">Should contain .Content</param>
    /// <returns>A path to the file</returns>
    Task<string> StoreResponseContentInTemp(HttpResponseMessage response, string fileName);
}

public class StorageService : IStorageService
{
    private readonly IOptions<Settings> _settings;

    public StorageService(IOptions<Settings> settings)
    {
        _settings = settings;
    }

    public async Task<string> StoreResponseContentInTemp(HttpResponseMessage response, string fileName)
    {
        var directoryPath = _settings.Value.TempFolderPath;

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var fullPath = Path.Combine(directoryPath, fileName);

        await using var fileStream =  new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
        return fullPath;
    }
}
