using System.IO.Abstractions;
using System.Net;
using aia_api.Application.Handlers.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace TestProject.Application.FileHandler;

public class LlmFileUploaderHandlerTest
{
    private PredictionDatabaseService _predictionDatabaseService;
    private PredictionDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<PredictionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new PredictionDbContext(dbContextOptions);
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<PredictionDatabaseService>>();
        mockServiceProvider.Setup(x => x.GetService(typeof(PredictionDbContext))).Returns(_dbContext);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        _predictionDatabaseService = new PredictionDatabaseService(mockLogger.Object, serviceScopeFactory.Object);
    }

    [Test]
    public async Task Handle_ValidInput_ReturnsHandlerResult()
    {
        // Arrange
        var fileName = "testzip.zip";
        var inputPathFolder = "./Testfiles/";
        var inputContentType = "application/zip";
        var outputFolderPath = "./Testfiles/Output/";
        var inputPath = Path.Combine(inputPathFolder, fileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { OutputFolderPath = outputFolderPath });
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
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(), _predictionDatabaseService);

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
        var inputPathFolder = "./Testfiles/";
        var inputContentType = "application/zip";
        var outputFolderPath = "./Testfiles/Output/";
        var inputPath = Path.Combine(inputPathFolder, fileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { OutputFolderPath = outputFolderPath });
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
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(), _predictionDatabaseService);

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
        var inputPathFolder = "./Testfiles/";
        var inputContentType = "application/zip";
        var outputFolderPath = "./Testfiles/Output/";
        var inputPath = Path.Combine(inputPathFolder, zipFileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { OutputFolderPath = outputFolderPath });
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
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(), _predictionDatabaseService);

        // Act
        await handler.Handle(inputPath, inputContentType);
        var retrievedPredictions = _dbContext.Predictions.ToList();

        // Assert
        Assert.That(retrievedPredictions, Is.Not.Null);
        Assert.That(retrievedPredictions, Is.Not.Empty);

    }

     [Test]
    public async Task Handle_CreatesDbPredictions_WithContent()
    {
        // Arrange
        var zipFileName = "testzip.zip";
        var inputPathFolder = "./Testfiles/";
        var inputContentType = "application/zip";
        var outputFolderPath = "./Testfiles/Output/";
        var inputPath = Path.Combine(inputPathFolder, zipFileName);

        var loggerMock = new Mock<ILogger<LlmFileUploaderHandler>>();
        var settings = Options.Create(new Settings { OutputFolderPath = outputFolderPath });
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
        var handler = new LlmFileUploaderHandler(loggerMock.Object, settings, replicateSettings, replicateApi.Object, new FileSystem(), _predictionDatabaseService);

        // Act
        await handler.Handle(inputPath, inputContentType);
        var retrievedPredictions = _dbContext.Predictions.ToList();

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
