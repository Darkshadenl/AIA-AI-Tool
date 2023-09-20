using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Services;

public interface IStorageService
{
    /// <summary>
    /// Stores the content of a response in the temp folder.
    /// Configure this folder in appsettings.json
    /// </summary>
    /// <param name="input">Should contain .Content</param>
    /// <returns>Filename of the outputfile</returns>
    Task<string> StoreInTemp(HttpResponseMessage input, string fileName);

    /// <summary>
    /// Stores the content of a stream in the temp folder.
    /// Configure this folder in appsettings.json
    /// </summary>
    /// <param name="input">Any stream</param>
    /// <returns>Filename of the outputfile</returns>
    Task<string> StoreInTemp(Stream input, string fileName);
}

public class StorageService : IStorageService
{
    private readonly IOptions<Settings> _settings;

    public StorageService(IOptions<Settings> settings)
    {
        _settings = settings;
    }

    public async Task<string> StoreInTemp(HttpResponseMessage input, string fileName)
    {
        Stream responseStream = await input.Content.ReadAsStreamAsync();
        return await StoreInTemp(responseStream, fileName);
    }

    public async Task<string> StoreInTemp(Stream input, string fileName)
    {
        var directoryPath = _settings.Value.TempFolderPath;
        var fullPath = Path.Combine(directoryPath, fileName);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await input.CopyToAsync(fileStream);
        return fullPath;
    }


}
