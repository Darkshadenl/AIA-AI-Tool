using System.IO.Abstractions;
using System.Net;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework.Internal;

namespace TestProject.Application.FileHandler;

public class LlmFileUploaderHandlerTest
{
    private DbContextOptions<PredictionDbContext> CreateDbContextOptions()
    {
        var options = new DbContextOptionsBuilder<PredictionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return options;
    }

    [Test]
    public async Task Handle_ValidInput_ReturnsHandlerResult()
    {
        // Arrange
        var fileName = "testzip.zip";
        var inputPathFolder = "/Users/quintenmeijboom/Documents/Repos/aia_api/TestProject/Testfiles/";
        var inputContentType = "application/zip";
        var inputPath = Path.Combine(inputPathFolder, fileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { TempFolderPath = inputPathFolder });
        var replicateSettings = Options.Create(
            new ReplicateSettings { Prompt = "prompt", WebhookUrl = "webhookUrl" });

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{'id':1,'value':'1'}]"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("http://localhost");
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var replicateApi = new Mock<ReplicateApi>(clientFactory.Object, replicateSettings);
        var dbContext = new Mock<PredictionDbContext>(CreateDbContextOptions());
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(),
            (InterfacesAia.IPredictionDatabaseService) dbContext.Object);

        // Act
        var result = await handler.Handle(inputPath, inputContentType);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.ErrorMessage, Is.EqualTo("OK"));
        });
    }

    [Test]
    public async Task Handle_ErrorsMoreThanZero_ReturnsSuccessFalseHandlerResult()
    {
        // Arrange
        var fileName = "testzip.zip";
        var inputPathFolder = "/Users/quintenmeijboom/Documents/Repos/aia_api/TestProject/Testfiles/";
        var inputContentType = "application/zip";
        var inputPath = Path.Combine(inputPathFolder, fileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { TempFolderPath = inputPathFolder });
        var replicateSettings = Options.Create(
            new ReplicateSettings { Prompt = "prompt", WebhookUrl = "webhookUrl" });

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("some_content"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("http://localhost");
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var replicateApi = new Mock<ReplicateApi>(clientFactory.Object, replicateSettings);
        var dbContext = new Mock<PredictionDbContext>(CreateDbContextOptions());
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(),
            (InterfacesAia.IPredictionDatabaseService)dbContext.Object);

        // Act
        var result = await handler.Handle(inputPath, inputContentType);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result.ErrorMessage, Is.Not.Empty);
        });
    }

    [Test]
    public async Task Handle_CreatesDbPredictions_DbContainsPrediction()
    {
        // Arrange
        var zipFileName = "testzip.zip";
        var inputPathFolder = "/Users/quintenmeijboom/Documents/Repos/aia_api/TestProject/Testfiles/";
        var inputContentType = "application/zip";
        var inputPath = Path.Combine(inputPathFolder, zipFileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { TempFolderPath = inputPathFolder });
        var replicateSettings = Options.Create(
            new ReplicateSettings { Prompt = "prompt", WebhookUrl = "webhookUrl" });

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("some_content"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("http://localhost");
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var replicateApi = new Mock<ReplicateApi>(clientFactory.Object, replicateSettings);
        var dbContext = new PredictionDbContext(CreateDbContextOptions());

        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(), (InterfacesAia.IPredictionDatabaseService)dbContext);

        // Act
        await handler.Handle(inputPath, inputContentType);
        var retrievedPredictions = dbContext.Predictions.ToList();

        // Assert
        Assert.That(retrievedPredictions, Is.Not.Null);
        Assert.That(retrievedPredictions, Is.Not.Empty);

    }

     [Test]
    public async Task Handle_CreatesDbPredictions_WithContent()
    {
        // Arrange
        var zipFileName = "testzip.zip";
        var inputPathFolder = "/Users/quintenmeijboom/Documents/Repos/aia_api/TestProject/Testfiles/";
        var inputContentType = "application/zip";
        var inputPath = Path.Combine(inputPathFolder, zipFileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { TempFolderPath = inputPathFolder });
        var replicateSettings = Options.Create(
            new ReplicateSettings { Prompt = "prompt", WebhookUrl = "webhookUrl" });

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("some_content"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("http://localhost");
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var replicateApi = new Mock<ReplicateApi>(clientFactory.Object, replicateSettings);
        var dbContext = new PredictionDbContext(CreateDbContextOptions());

        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(),
            (InterfacesAia.IPredictionDatabaseService)dbContext);

        // Act
        await handler.Handle(inputPath, inputContentType);
        var retrievedPredictions = dbContext.Predictions.ToList();

        // Assert
        foreach (var prediction in retrievedPredictions)
        {
            Assert.Multiple(() =>
            {
                Assert.That(prediction.FileName, Is.Not.Null);
                Assert.That(prediction.FileExtension, Is.Not.Null);
                Assert.That(prediction.Prompt, Is.Not.Null);
            });
        }
    }
}
