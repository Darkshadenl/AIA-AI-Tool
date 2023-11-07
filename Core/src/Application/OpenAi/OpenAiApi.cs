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
    private readonly ILogger<OpenAiApi> _logger;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ISignalRService _signalRService;
    private readonly CommentManipulationHelper _commentManipulationHelper;
    private readonly IPredictionDatabaseService _predictionDatabaseService;
    private readonly FindFileDifferenceHelper _differenceHelper;
    
    public OpenAiApi(
        ILogger<OpenAiApi> logger,
        IOptions<OpenAiSettings> openAiSettings,
        ISignalRService signalRService,
        CommentManipulationHelper commentManipulationHelper,
        IPredictionDatabaseService predictionDatabaseService,
        FindFileDifferenceHelper differenceHelper
        )
    {
        _logger = logger;
        _openAiSettings = openAiSettings.Value;
        _signalRService = signalRService;
        _commentManipulationHelper = commentManipulationHelper;
        _predictionDatabaseService = predictionDatabaseService;
        _differenceHelper = differenceHelper;
    }
    
    public async Task<ChatChoice> SendOpenAiCompletion(IDbPrediction dbPrediction)
    {
        ChatCompletionsOptions options = CreateChatCompletionsOptions(dbPrediction.Prompt);
        OpenAIClient openAiClient = new OpenAIClient(_openAiSettings.ApiToken);
        Response<ChatCompletions> response = await openAiClient.GetChatCompletionsAsync(_openAiSettings.ModelName, options);
        _logger.LogDebug("LLM usage for {fileName} was {usage}", dbPrediction.FileName, response.Value.Usage);
        return response.Value.Choices.First();
    }

    public void ProcessApiResponse(ChatChoice openAiResponse, IDbPrediction dbPrediction)
    {
        string codeWithComments = 
            _commentManipulationHelper.ReplaceCommentInCode(openAiResponse.Message.Content, dbPrediction.InputCode);
        
        
        _predictionDatabaseService.UpdatePredictionResponseText(dbPrediction, openAiResponse.Message.Content);
        _predictionDatabaseService.UpdatePredictionEditedResponseText(dbPrediction, codeWithComments);
        
        _differenceHelper.FindDifferences(dbPrediction.InputCode, dbPrediction.EditedResponseText);
        
        _signalRService.SendLlmResponseToFrontend(dbPrediction.ClientConnectionId, dbPrediction.FileName, dbPrediction.FileExtension, codeWithComments, dbPrediction.InputCode);
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
            Temperature = _openAiSettings.Temperature,
            MaxTokens = _openAiSettings.MaxTokens
        };
    }
}