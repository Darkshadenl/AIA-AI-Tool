using aia_api.Application.FileHandler;
using aia_api.Configuration.Azure;
using aia_api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject.Application.FileHandler;

public class AzureUploaderTest
{
    private Mock<IOptions<Settings>> _mockSettings;
    private Mock<AzureService> _mockAzureService;
    private UploadHandler _uploadHandler;

    [SetUp]
    public void SetUp()
    {
        _mockSettings = new Mock<IOptions<Settings>>();
        _mockAzureService = new Mock<AzureService>();
        _uploadHandler = new UploadHandler(_mockSettings.Object);
        _uploadHandler.SetClient(_mockAzureService.Object);
    }

    [Test]
    public async Task Handle_UploadsFileAndLogsSuccessMessage()
    {
        // Arrange
        _mockSettings.Setup(s => s.Value).Returns(new Settings { OutputFolderPath = "some/output/path" });

        // Act
        await _uploadHandler.Handle("some/input/path/file.zip", "application/zip");

        // Assert
        _mockAzureService.Verify(a => a.Pipeline(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        // Add assertion to check if success message is logged (depends on your logging setup)
    }

    [Test]
    public async Task Handle_CatchesIOExceptionAndLogsMessage()
    {
        // Arrange
        _mockAzureService.Setup(a => a.Pipeline(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new IOException("Some IO Exception"));

        // Act
        await _uploadHandler.Handle("some/input/path/file.zip", "application/zip");

        // Assert
        // Add assertion to check if IO exception message is logged (depends on your logging setup)
    }

    [Test]
    public void Handle_RethrowsOtherExceptions()
    {
        // Arrange
        _mockAzureService.Setup(a => a.Pipeline(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Some Exception"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _uploadHandler.Handle("some/input/path/file.zip", "application/zip"));
    }
}
