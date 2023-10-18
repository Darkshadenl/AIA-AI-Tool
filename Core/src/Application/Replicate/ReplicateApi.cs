using System.Net.Http.Headers;
using System.Text;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace aia_api.Application.Replicate;

public class ReplicateApi
{
    private readonly HttpClient _httpClient;
    private readonly ReplicateSettings _replicateSettings;

    public ReplicateApi(IOptions<ReplicateSettings> replicateSettings, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _replicateSettings = replicateSettings.Value;
    }

    public async Task<HttpResponseMessage> RunPrediction(Prediction prediction)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
            _replicateSettings.ApiToken);

        var serializeObject = JsonConvert.SerializeObject(prediction);
        var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");

        return await _httpClient.PostAsync(_replicateSettings.ReplicateUrl, content);
    }

}
