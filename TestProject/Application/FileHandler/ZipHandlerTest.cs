using aia_api.Application.FileHandler;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject.Application.FileHandler;

public class ZipHandlerTest
{
    private Mock<IOptions<Settings>> mockSettings;
    private ZipHandler zipHandler;

    [SetUp]
    public void SetUp()
    {
        mockSettings = new Mock<IOptions<Settings>>();
        mockSettings.Setup(s => s.Value).Returns(new Settings { OutputFolderPath = "some/temp/path" });
        zipHandler = new ZipHandler(mockSettings.Object);
    }

    [Test]
    public async Task Handle_ProcessesZipFileCorrectly()
    {
        // Arrange
        var nextHandlerMock = new Mock<AbstractFileHandler>(mockSettings.Object);
        zipHandler.SetNext(nextHandlerMock.Object);

        // Act
        await zipHandler.Handle("somefile.zip", "application/zip");

        // Assert
        // Add your assertions here, for example:
        // nextHandlerMock.Verify(x => x.Handle(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Handle_ForwardsToNextHandler_WhenNotZip()
    {
        // Arrange
        var nextHandlerMock = new Mock<AbstractFileHandler>(mockSettings.Object);
        zipHandler.SetNext(nextHandlerMock.Object);

        // Act
        await zipHandler.Handle("somefile.txt", "text/plain");

        // Assert
        nextHandlerMock.Verify(x => x.Handle("somefile.txt", "text/plain"), Times.Once);
    }

    [Test]
    public void Handle_ThrowsException_WhenNoNextHandler()
    {
        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => zipHandler.Handle("somefile.txt", "text/plain"));
    }
}
