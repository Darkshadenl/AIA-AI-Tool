using aia_api.Application.Helpers;
using aia_api.Configuration.Records;
using Azure;
using Azure.AI.OpenAI;
using InterfacesAia.Database;
using InterfacesAia.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Application.OpenAi;

public class OpenAiApi
{
    private readonly OpenAiSettings _openAiSettings;
    private readonly ISignalRService _signalRService;
    private readonly CommentManipulationHelper _commentManipulationHelper;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    
    public OpenAiApi(
        IOptions<OpenAiSettings> openAiSettings,
        ISignalRService signalRService,
        CommentManipulationHelper commentManipulationHelper,
        IPredictionDatabaseService predictionDatabaseService
        )
    {
        _openAiSettings = openAiSettings.Value;
        _signalRService = signalRService;
        _commentManipulationHelper = commentManipulationHelper;
        _predictionDatabaseService = predictionDatabaseService;
    }
    
    public async Task<ChatChoice> SendOpenAiCompletion(IDbPrediction dbPrediction)
    {
        ChatCompletionsOptions options = CreateChatCompletionsOptions(dbPrediction.Prompt);
        OpenAIClient openAiClient = new OpenAIClient(_openAiSettings.ApiToken);
        Response<ChatCompletions> response = await openAiClient.GetChatCompletionsAsync(_openAiSettings.ModelName, options);
        return response.Value.Choices.First();
    }

    public void ProcessApiResponse(ChatChoice openAiResponse, IDbPrediction dbPrediction)
    {
        string codeWithComments = 
            _commentManipulationHelper.ReplaceCommentInCode(openAiResponse.Message.Content, dbPrediction.InputCode);
                
        _predictionDatabaseService.UpdatePrediction(dbPrediction, codeWithComments);
        _signalRService.SendLlmResponseToFrontend(dbPrediction.FileName, dbPrediction.FileExtension, codeWithComments);
    }

    private ChatCompletionsOptions CreateChatCompletionsOptions(string prompt)
    {
        return new ChatCompletionsOptions(new[]
        {
            new ChatMessage
            {
                Role = ChatRole.System,
                Content = _openAiSettings.SystemPrompt
            },
            new ChatMessage { Role = ChatRole.User, Content = prompt },
        })
        {

            Temperature = 0.5f,
            PresencePenalty = 0,
            FrequencyPenalty = 0,
            ChoiceCount = 1
        };
    }
}