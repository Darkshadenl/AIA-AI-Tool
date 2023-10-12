using System.IO.Abstractions;
using System.Net;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using Microsoft.Extensions.Options;
using Moq;

namespace TestProject.Application.FileHandler;

public class LlmFileUploaderHandlerTest
{
    [Test]
    public async Task Handle_ValidInput_ReturnsHandlerResult()
    {
        // Arrange
        var inputPath = "inputPath";
        var inputContentType = "inputContentType";
        var settings = Options.Create(new Settings { OutputFolderPath = "outputFolderPath" });
        var fileSystem = new Mock<IFileSystem>();
        var replicateApi = new Mock<ReplicateApi>();
        var dbContext = new Mock<PredictionDbContext>();
        var replicateSettings = Options.Create(new ReplicateSettings { Prompt = "prompt", WebhookUrl = "webhookUrl" });
        var handler = new LlmFileUploaderHandler(settings, replicateSettings, replicateApi.Object, fileSystem.Object, dbContext.Object);

        // Act
        var result = await handler.Handle(inputPath, inputContentType);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual("OK", result.ErrorMessage);
    }
}
