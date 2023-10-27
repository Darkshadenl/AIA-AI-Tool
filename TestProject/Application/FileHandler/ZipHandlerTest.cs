using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using aia_api.Application.Handlers.FileHandler;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject.Application.FileHandler;

public class ZipHandlerTest
{
    private Mock<IOptions<Settings>> _settingsMock;
    private Mock<ILogger<ZipHandler>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _settingsMock = new Mock<IOptions<Settings>>();
        _settingsMock.Setup(s => s.Value).Returns(new Settings
        {
            OutputFolderPath = "some/temp/path",
            AllowedFileTypes = new []{ ".zip "}
        });
        _loggerMock = new Mock<ILogger<ZipHandler>>();
    }

    [Test]
    public async Task Handle_ProcessesZipFileCorrectly()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        byte[] zipData;
        using (MemoryStream ms = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                ZipArchiveEntry entry = archive.CreateEntry("test.txt");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write("Test content");
                }
            }
            zipData = ms.ToArray();
        }

        mockFs.AddFile("somefile.zip", new MockFileData(zipData));
        
        var abstractFileHandlerLoggerMock = new Mock<ILogger<AbstractFileHandler>>();
        var zipHandler = new ZipHandler(_loggerMock.Object, _settingsMock.Object, mockFs);
        var nextHandlerMock = new Mock<AbstractFileHandler>(abstractFileHandlerLoggerMock.Object, _settingsMock.Object);
        zipHandler.SetNext(nextHandlerMock.Object);

        // Act
        await zipHandler.Handle("somefile.zip", "application/zip");

        // Assert
        nextHandlerMock.Verify(x => x.Handle(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Handle_ForwardsToNextHandler_WhenNotZip()
    {
        // Arrange
        var abstractFileHandlerLoggerMock = new Mock<ILogger<AbstractFileHandler>>();
        var zipHandler = new ZipHandler(_loggerMock.Object, _settingsMock.Object, new MockFileSystem());
        var nextHandlerMock = new Mock<AbstractFileHandler>(abstractFileHandlerLoggerMock.Object, _settingsMock.Object);
        zipHandler.SetNext(nextHandlerMock.Object);

        // Act
        await zipHandler.Handle("somefile.txt", "text/plain");

        // Assert
        nextHandlerMock.Verify(x => x.Handle("somefile.txt", "text/plain"), Times.Once);
    }

    [Test]
    public void Handle_ShouldNotThrow_WhenNoNextHandler()
    {
        // Arrange
        var zipHandler = new ZipHandler(_loggerMock.Object, _settingsMock.Object, new MockFileSystem());

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await zipHandler.Handle("somefile.txt", "text/plain"));
    }
}
