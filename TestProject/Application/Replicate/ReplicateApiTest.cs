using System.Net;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace TestProject.Application.Replicate;

public class ReplicateApiTest
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;

    [SetUp]
    public void SetUp()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    }

    [Test]
    public async Task RunPrediction_ShouldHandleUnauthorized_WhenApiTokenIsInvalid()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<ReplicateSettings>>();
        var replicateSettings = new ReplicateSettings
        {
            ApiToken = "your_specific_value_here",
        };

        mockSettings.Setup(m => m.Value).Returns(replicateSettings);

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var mockPrediction = new ReplicatePredictionDto(
            version: "version",
            input: new CodeLLamaPredictionInputDto(
                prompt: "prompt",
                max_tokens: 500,
                temperature: 0.8,
                top_p: 0.95,
                top_k: 10,
                frequency_penalty: 0,
                presence_penalty: 0,
                repeat_penalty: 1.1
            ),
            webhook: "hook");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://api.replicate.com");
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var replicateApi = new ReplicateApi(httpClientFactory.Object, mockSettings.Object);

        // Act
        var result = await replicateApi.SendPrediction(mockPrediction);

        // Assert
        Assert.That(result is not null);
        Assert.That(result.IsSuccessStatusCode, Is.False);
    }



}
