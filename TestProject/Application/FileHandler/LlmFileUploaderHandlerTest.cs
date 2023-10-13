using System.Collections;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using InterfacesAia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace TestProject.Application.FileHandler;

public class LlmFileUploaderHandlerTest
{
    private DbContextOptions<PredictionDbContext> CreateDbContextOptions()
    {
        var options = new DbContextOptionsBuilder<PredictionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique name for each test
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
        var outputFolderPath = "/Users/quintenmeijboom/Documents/Repos/aia_api/TestProject/Testfiles/Output/";
        var inputPath = Path.Combine(inputPathFolder, fileName);

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
        var dbContext = new Mock<PredictionDbContext>(settings, CreateDbContextOptions());
        var handler = new LlmFileUploaderHandler(settings, replicateSettings, replicateApi.Object, new FileSystem(), dbContext.Object);

        // Act
        var result = await handler.Handle(inputPath, inputContentType);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual("OK", result.ErrorMessage);
    }
}
