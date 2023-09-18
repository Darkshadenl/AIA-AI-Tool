using NGitLab;

namespace aia_api.Application.Gitlab;

public interface IGitLabClientFactory
{
    GitLabClient CreateClient(string apiToken);
}

public class GitLabClientFactory : IGitLabClientFactory
{
    public GitLabClient CreateClient(string apiToken)
    {
        return new GitLabClient("https://gitlab.example.com", apiToken);
    }
}
