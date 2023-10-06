using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using aia_api.Application.FileHandler;
using aia_api.Configuration.Azure;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject.Application.FileHandler;

public class ZipHandlerTest
{
    private Mock<IOptions<Settings>> mockSettings;

    [SetUp]
    public void SetUp()
    {
        mockSettings = new Mock<IOptions<Settings>>();
        mockSettings.Setup(s => s.Value).Returns(new Settings
        {
            OutputFolderPath = "some/temp/path",
            AllowedFiles = new []{ ".zip "}
        });
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

        var zipHandler = new ZipHandler(mockSettings.Object, mockFs);
        var nextHandlerMock = new Mock<AbstractFileHandler>(mockSettings.Object);
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
        var zipHandler = new ZipHandler(mockSettings.Object, new MockFileSystem());
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
        // Arrange
        var zipHandler = new ZipHandler(mockSettings.Object, new MockFileSystem());

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => zipHandler.Handle("somefile.txt", "text/plain"));
    }
}
