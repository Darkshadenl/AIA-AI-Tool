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
    private ReplicateApi _replicateApi;

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
            ReplicateUrl = "https://replicate.ai/api/v1/models/predictions",
        };

        mockSettings.Setup(m => m.Value).Returns(replicateSettings);


        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var mockPrediction = new Prediction(
            version: "version",
            input: new PredictionInput(
                prompt: "prompt",
                max_tokens: 500,
                temperature: 0.8,
                top_p: 0.95,
                top_k: 10,
                frequency_penalty: 0,
                presence_penalty: 0,
                repeat_penalty: 1.1
            ),
            webhook: "hook",
            webhook_events_filter: new[] { "completed" }
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _replicateApi = new ReplicateApi(mockSettings.Object, httpClient);

        // Act
        var result = await _replicateApi.RunPrediction(mockPrediction);

        // Assert
        Assert.That(result is not null);
        Assert.That(result.IsSuccessStatusCode, Is.False);
    }



}
