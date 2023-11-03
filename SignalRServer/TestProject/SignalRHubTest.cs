using Moq;
using SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace TestProject;

public class SignalRHubTest
{
    private static Mock<IMainHub> _mainHubMock;
    private static Mock<IHubCallerClients<IMainHub>> _mockClientsMock;
    private static MainHub _hub;

    private static string _fileName;
    private static string _base64;
    private static string _contentType;
    private static byte[] _byteArray;
    private static string _clientConnectionId;


    [SetUp]
    public void Setup()
    {
        _mainHubMock = new Mock<IMainHub>();
        _mockClientsMock = new Mock<IHubCallerClients<IMainHub>>();
        _mockClientsMock.Setup(_ => _.Others).Returns(_mainHubMock.Object);
        _mockClientsMock.Setup(_ => _.Caller).Returns(_mainHubMock.Object);

        var loggerMock = new Mock<ILogger<MainHub>>();
        _hub = new MainHub(loggerMock.Object)
        {
            Clients = _mockClientsMock.Object,
        };

        _fileName = "TestFile.zip";
        _contentType = "application/zip";
        _base64 = "UEsDBBQACAAIAPhjS1cAAAAAAAAAACoBAAAHACAAdGVzdC5jc1VUDQAHlHkmZZV5JmWUeSZldXgLAAEE9gEAAAQUAAAAXY7BDoIwEETv/Yo9QiQSTThx9A+Uu1nqEpq0pem28WD4d4WmoM5tZ152Rlg0xA4lASq8o1PHK3HU4diRca0QUiMzXFDLqDGQeAn4yMVeK7nbRbnaKVxU19CNimGIVgY1WZAZZQgjAUcD0wDhOYGNpifPgPYBnkL0NiF+2bE93Kpu0RRNBU3ZrtksvgcpG37JxUgFpwr24/y/NxVnFA6ZyyWzeANQSwcIe7qJr6QAAAAqAQAAUEsDBBQACAAIAAVsS1cAAAAAAAAAAIMAAAAHACAAdGVzdC5qc1VUDQAHuocmZbyHJmW6hyZldXgLAAEE9gEAAAQUAAAA49LXVwjJyCxWAKJEheT83NzUvBKutNK85JLM/DyF5MSc5NKcxJLU4NJcjbzS3KTUIkMdBQjDSFOhmksBCIpSS0qL8qCihgraMHlrrlouLhQTTHUUTDWtFYB2+peWFJSWWCkYGnABAFBLBwjIqlIfZwAAAIMAAABQSwMEFAAIAAgAKWJLVwAAAAAAAAAArgAAAAcAIAB0ZXN0LnB5VVQNAAcvdiZlMHYmZS92JmV1eAsAAQT2AQAABBQAAABVjbEKg0AQRPv9ikGbSGwM2ASs8gvpl8tljxx4K9zt4e9HohaZcuYxj1o83Ozr7ExgH0GpCUuArQu0ppfkQm8J8CfEG3DZl6E/kFt3J2zJYjXrUQ64njNRi6cU+wlCVW9xUYoBzOqSMGOa0DAnF5W52c/+jWOPsaMvUEsHCJloci55AAAArgAAAFBLAQIUAxQACAAIAPhjS1d7uomvpAAAACoBAAAHACAAAAAAAAAAAACkgQAAAAB0ZXN0LmNzVVQNAAeUeSZllXkmZZR5JmV1eAsAAQT2AQAABBQAAABQSwECFAMUAAgACAAFbEtXyKpSH2cAAACDAAAABwAgAAAAAAAAAAAApIH5AAAAdGVzdC5qc1VUDQAHuocmZbyHJmW6hyZldXgLAAEE9gEAAAQUAAAAUEsBAhQDFAAIAAgAKWJLV5loci55AAAArgAAAAcAIAAAAAAAAAAAAKSBtQEAAHRlc3QucHlVVA0ABy92JmUwdiZlL3YmZXV4CwABBPYBAAAEFAAAAFBLBQYAAAAAAwADAP8AAACDAgAAAAA=";
        _byteArray = Convert.FromBase64String(_base64);
        _clientConnectionId = Guid.NewGuid().ToString();
    }

    [TearDown]
    public void TearDown()
    {
        _hub.Dispose();
    }

    [Test]
    public async Task MainHub_UploadZip_Success()
    {
        await _hub.UploadChunk(_clientConnectionId, _fileName, _contentType, _base64, 0, 1);
        _mainHubMock.Verify(_ => _.UploadChunk(It.IsAny<string>(), _fileName, _contentType, _byteArray, It.IsAny<int>(), It.IsAny<int>()));
    }

    [Test]
    public async Task MainHub_UploadZip_Fail_Empty_Params()
    {
        await _hub.UploadChunk(_clientConnectionId, "", "", "", 0, 1);
        _mainHubMock.Verify(_ => _.ReceiveError(_clientConnectionId, "No file received or file is empty."));
    }

    [Test]
    public async Task MainHub_UploadZip_Fail_Unexpected_DataUri()
    {
        string unexpectedDataUri = "Unexpected";

        await _hub.UploadChunk(_clientConnectionId, _fileName, _contentType, unexpectedDataUri, 0, 1);
        _mainHubMock.Verify(_ => _.ReceiveError(_clientConnectionId, "The file is not converted to base64 correctly."));
    }
}