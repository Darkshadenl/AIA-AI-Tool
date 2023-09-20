using aia_api.Application.Helpers.Factories;
using aia_api.Services;
using Moq;

namespace TestProject.Application.Gitlab;

public class GitlabApiTest
{
    [Test]
    public async Task DownloadRepository_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockStreamFactory = new Mock<IStreamFactory>();
        var gitlabApi = new GitlabService(mockHttpClient.Object, mockStreamFactory.Object);

        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), path);

        // Act
        await gitlabApi.DownloadRepository("some_project_id", "some_api_token");

        // Assert
        Assert.IsTrue(Directory.Exists("expected_directory_path"));
    }

    [Test]
    public void DownloadRepository_ThrowsExceptionOnFailedDownload()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        // ... setup mockHttpClient to return a failed response
        var service = new YourServiceClass(mockHttpClient.Object);

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => service.DownloadRepository("some_project_id", "some_api_token"));
    }

    [Test]
    public async Task DownloadRepository_WritesToFileStream()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        // ... setup mockHttpClient to return a successful response with some content
        var service = new YourServiceClass(mockHttpClient.Object);

        // Act
        var path = await service.DownloadRepository("some_project_id", "some_api_token");

        // Assert
        Assert.IsTrue(File.Exists(path));
        // Optionally, you can also check the content of the file
    }



}
