using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using Azure;
using Azure.AI.OpenAI;
using InterfacesAia.Database;
using Microsoft.Extensions.Options;

namespace aia_api.Application.OpenAi;

public class OpenAiApi
{
    private readonly OpenAiSettings _openAiSettings;
    
    public OpenAiApi(IOptions<OpenAiSettings> openAiSettings)
    {
        _openAiSettings = openAiSettings.Value;
    }
    
    public async Task<Response<ChatCompletions>> CreateOpenAiPrediction(IDbPrediction dbPrediction, string webHookWithId)
    {
        ChatCompletionsOptions options = new ChatCompletionsOptions(new[]
        {
            new ChatMessage { Role = ChatRole.System, Content = "You are a a coding master." },
            new ChatMessage { Role = ChatRole.User, Content = _openAiSettings.Message }
        });
        OpenAIClient openAiClient = new OpenAIClient(_openAiSettings.ApiToken);
        Response<ChatCompletions> response = await openAiClient.GetChatCompletionsAsync(_openAiSettings.ModelName, options);
        Console.WriteLine(response.Value);
        return response;
    }
}