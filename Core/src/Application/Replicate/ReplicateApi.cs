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

    public async Task<Task> RunPrediction(Prediction prediction)
    {
        using (_httpClient)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
                _replicateSettings.ApiToken);

            var serializeObject = JsonConvert.SerializeObject(prediction);
            var content = new StringContent(serializeObject, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_replicateSettings.ReplicateUrl, content);
            Console.WriteLine($"response statuscode: {response.StatusCode}");
        }
        return Task.CompletedTask;
    }

}
