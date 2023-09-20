namespace aia_api.Application.Gitlab;

public class GitlabApi
{
    private readonly HttpClient _httpClient;

    public GitlabApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Downloads a repository from GitLab using the provided project ID and API token.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="apiToken">The API token for authentication.</param>
    /// <param name="path">The path where the repository will be downloaded (default is "TempDownloads").</param>
    /// <returns>The path where the repository was downloaded.</returns>
    /// <exception cref="Exception">Thrown if the repository download fails.</exception>
    public async Task<string> DownloadRepository(string projectId, string apiToken, string path = "Temp")
    {
        var url = $"https://gitlab.com/api/v4/projects/{projectId}/repository/archive.zip";

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), path);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss").Replace(" ", "_");
        var fileName = $"{projectId}_{date}.zip";
        var fullPath = Path.Combine(directoryPath, fileName);

        using var downloadClient = _httpClient;
        downloadClient.DefaultRequestHeaders.Add("Private-Token", apiToken);

        using var response = await downloadClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Could not download repository.");

        await using var fileStream =
            new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
        return fullPath;
    }

}
