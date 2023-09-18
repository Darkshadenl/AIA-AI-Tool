using System.Text;
using aia_api.Application.Azure;
using aia_api.Configuration.Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace TestProject.Application.Azure;

[TestFixture]
public class AzureClientTest
{
    private Mock<BlobServiceClient> _blobServiceClientMock;
    private Mock<IOptions<AzureBlobStorageSettings>> _settingsMock;
    private AzureClient _azureClient;

    [SetUp]
    public void Setup()
    {
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _settingsMock = new Mock<IOptions<AzureBlobStorageSettings>>();
        _settingsMock.Setup(s => s.Value).Returns(new AzureBlobStorageSettings { BlobContainerName = "testContainer" });

        _azureClient = new AzureClient(_blobServiceClientMock.Object, _settingsMock.Object);
    }

    [Test]
    public async Task Pipeline_ShouldCallBlobClientMethods()
    {
        // Arrange
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));
        var fileName = "testFile.zip";
        var blobContainerName = "testContainer";

        var blobWriteStreamMock = new Mock<Stream>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();
        _blobServiceClientMock.Setup(x => x.GetBlobContainerClient(blobContainerName))
            .Returns(blobContainerClientMock.Object);
        blobContainerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
        blobClientMock.Setup(x => x.OpenWriteAsync(true, default, default)).ReturnsAsync(blobWriteStreamMock.Object);

        // Act
        await _azureClient.Pipeline(memoryStream, fileName);

        // Assert
        _blobServiceClientMock.Verify(x => x.GetBlobContainerClient(blobContainerName), Times.Once);
        blobClientMock.Verify(x => x.OpenWriteAsync( true, default, default), Times.Once);
    }

    [Test]
    public async Task Pipeline_ReadsFromStream()
    {
        // Arrange
        var memoryStream = new Mock<MemoryStream>();
        var fileName = "testFile.zip";
        var blobContainerName = "testContainer";
        var blobWriteStreamMock = new Mock<Stream>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();

        memoryStream.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(0);

        _blobServiceClientMock.Setup(x => x.GetBlobContainerClient(blobContainerName))
            .Returns(blobContainerClientMock.Object);
        blobContainerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
        blobClientMock.Setup(x => x.OpenWriteAsync(true, default, default)).ReturnsAsync(blobWriteStreamMock.Object);

        // Act
        await _azureClient.Pipeline(memoryStream.Object, fileName);

        // Assert
        memoryStream.Verify(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), default), Times.AtLeastOnce);
    }

    [Test]
    public async Task Pipeline_WriteToBlobClientOutputStream()
    {
        // Arrange
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));

        var fileName = "testFile.zip";
        var blobContainerName = "testContainer";
        var blobWriteStreamMock = new Mock<Stream>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();

        _blobServiceClientMock.Setup(x => x.GetBlobContainerClient(blobContainerName))
            .Returns(blobContainerClientMock.Object);
        blobContainerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
        blobClientMock.Setup(x => x.OpenWriteAsync(true, default, default)).ReturnsAsync(blobWriteStreamMock.Object);

        // Act
        await _azureClient.Pipeline(memoryStream, fileName);

        // Assert
        blobWriteStreamMock.Verify(x => x.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), default), Times.AtLeastOnce);
    }


    [Test]
    public async Task Pipeline_AsyncTimesThree()
    {
        // Arrange
        var memoryStream1 = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));
        var memoryStream2 = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));
        var memoryStream3 = new MemoryStream(Encoding.UTF8.GetBytes("Test content"));
        var fileName = "testFile.zip";
        var blobContainerName = "testContainer";

        var blobWriteStreamMock = new Mock<Stream>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();
        _blobServiceClientMock.Setup(x => x.GetBlobContainerClient(blobContainerName))
            .Returns(blobContainerClientMock.Object);
        blobContainerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
        blobClientMock.Setup(x => x.OpenWriteAsync(true, default, default)).ReturnsAsync(blobWriteStreamMock.Object);

        // Act
        var task1 = _azureClient.Pipeline(memoryStream1, fileName);
        var task2 = _azureClient.Pipeline(memoryStream2, fileName);
        var task3 = _azureClient.Pipeline(memoryStream3, fileName);

        await Task.WhenAll(task1, task2, task3);

        // Assert
        _blobServiceClientMock.Verify(x => x.GetBlobContainerClient(blobContainerName), Times.Exactly(3));
        blobClientMock.Verify(x => x.OpenWriteAsync( true, default, default), Times.Exactly(3));
    }


}
