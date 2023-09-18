namespace aia_api.Application.Gitlab;

public class GitlabApi
{
    private readonly GitLabClientFactory _clientFactory;
    private readonly HttpClient _httpClient;

    public GitlabApi(GitLabClientFactory factory, HttpClient httpClient)
    {
        _clientFactory = factory;
        _httpClient = httpClient;
    }

    public async Task DownloadRepository(string projectId, string apiToken)
    {
        var url = $"https://gitlab.com/api/v4/projects/{projectId}/repository/archive.zip";
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "TempDownloads");
        var fileName = $"{projectId}.zip";
        var fullPath = Path.Combine(directoryPath, fileName);

        using var downloadClient = _httpClient;
        downloadClient.DefaultRequestHeaders.Add("Private-Token", apiToken);

        using var response = await downloadClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Could not download repository.");

        await using var fileStream =
            new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
        Console.WriteLine("File downloaded.");

    }

}
