using System.Net.Http.Headers;
using System.Text;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace aia_api.Application.Replicate;

public class ReplicateApi
{
    private readonly ReplicateSettings _replicateSettings;

    public ReplicateApi(IOptions<ReplicateSettings> replicateSettings)
    {
        _replicateSettings = replicateSettings.Value;
    }

    public async Task<Task> RunPrediction(Prediction prediction)
    {
        using HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
            _replicateSettings.ApiToken);

        var ser = JsonConvert.SerializeObject(prediction);
        var content = new StringContent(ser, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(_replicateSettings.ReplicateUrl, content);
        Console.WriteLine($"response statuscode: {response.StatusCode}");
        return Task.CompletedTask;
    }

}
