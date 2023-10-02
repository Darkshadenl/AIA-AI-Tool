using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace aia_api.Application.Replicate;

public class ReplicateApi
{
    private readonly string _apiToken;

    public ReplicateApi(string apiToken)
    {
        _apiToken = apiToken;
    }


    public async Task<Task> RunPrediction(Prediction prediction)
    {
        using HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);

        var content = new StringContent(JsonConvert.SerializeObject(prediction), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(prediction.ReplicateUrl, content);
        return Task.CompletedTask;
    }



}
