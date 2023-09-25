namespace aia_api.Services;

public class GitlabService
{
    private readonly HttpClient _httpClient;
    private readonly IStorageService _storageService;

    public GitlabService(HttpClient httpClient, IStorageService storageService)
    {
        _httpClient = httpClient;
        _storageService = storageService;
    }

    /// <summary>
    /// Downloads a repository from GitLab using the provided project ID and API token.
    /// </summary>
    /// <param name="projectId">The ID of the project.</param>
    /// <param name="apiToken">The API token for authentication.</param>
    /// <param name="path">The path where the repository will be downloaded.</param>
    /// <returns>The path where the repository was downloaded.</returns>
    /// <exception cref="Exception">Thrown if the repository download fails.</exception>
    public async Task<string> DownloadRepository(string projectId, string apiToken)
    {
        var url = $"https://gitlab.com/api/v4/projects/{projectId}/repository/archive.zip";

        using var downloadClient = _httpClient;
        downloadClient.DefaultRequestHeaders.Add("Private-Token", apiToken);

        using var response = await downloadClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Could not download repository.");

        var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss").Replace(" ", "_");
        var fileName = $"{projectId}_{date}.zip";

        return await _storageService.StoreInTemp(response, fileName);
    }

}
