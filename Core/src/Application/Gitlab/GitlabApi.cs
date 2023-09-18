namespace aia_api.Application.Gitlab;

public class GitlabApi
{
    private readonly GitLabClientFactory _clientFactory;

    public GitlabApi(GitLabClientFactory factory)
    {
        _clientFactory = factory;
    }

}
