using System.Text;
using aia_api.Configuration.Records;
using InterfacesAia;
using InterfacesAia.Database;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace aia_api.Application.Replicate;

public class ReplicateApi : ILlmApi
{
    private readonly ReplicateSettings _replicateSettings;
    private readonly HttpClient _replicateHttpClient;

    public ReplicateApi(IHttpClientFactory httpClientFactory,  IOptions<ReplicateSettings> replicateSettings)
    {
        _replicateSettings = replicateSettings.Value;
        _replicateHttpClient = httpClientFactory.CreateClient("replicateClient");
    }

    public async Task<HttpResponseMessage> SendPrediction(IReplicatePredictionDto replicatePredictionDto)
    {
        var serializeObject = JsonConvert.SerializeObject(replicatePredictionDto);
        var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _replicateHttpClient.PostAsync("/v1/predictions", content);
        return response;
    }

    public ReplicatePredictionDto CreateCodeLlamaPrediction(IDbPrediction dbPrediction, string webHookWithId)
    {
        return new ReplicatePredictionDto(
            version: _replicateSettings.ModelVersion,
            input: new CodeLLamaPredictionInputDto(
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
