using System.Net.Http.Headers;
using System.Text;
using aia_api.Configuration.Records;
using aia_api.Database;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace aia_api.Application.Replicate;

public class ReplicateApi
{
    private readonly ReplicateSettings _replicateSettings;
    private readonly HttpClient _replicateHttpClient;

    public ReplicateApi(IHttpClientFactory httpClientFactory,  IOptions<ReplicateSettings> replicateSettings)
    {
        _replicateSettings = replicateSettings.Value;
        _replicateHttpClient = httpClientFactory.CreateClient("replicateClient");
    }

    public async Task<HttpResponseMessage> SendPrediction(PredictionDTO predictionDto)
    {
        var serializeObject = JsonConvert.SerializeObject(predictionDto);
        var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");
        var response = await _replicateHttpClient.PostAsync("/v1/predictions", content);
        return response;
    }

    public PredictionDTO CreateCodeLlamaPrediction(DbPrediction dbPrediction, string webHookWithId)
    {
        return new PredictionDTO(
            version: _replicateSettings.ModelVersion,
            InputDto: new CodeLLamaPredictionInputDto(
                prompt: dbPrediction.Prompt,
                max_tokens: 500,
                temperature: 0.8,
                top_p: 0.95,
                top_k: 10,
                frequency_penalty: 0,
                presence_penalty: 0,
                repeat_penalty: 1.1
            ),
            webhook: webHookWithId
        );
    }

}
