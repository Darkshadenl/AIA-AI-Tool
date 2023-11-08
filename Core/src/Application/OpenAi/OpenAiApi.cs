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
    
    public OpenAiApi(
        ILogger<OpenAiApi> logger,
        IOptions<OpenAiSettings> openAiSettings,
        ISignalRService signalRService,
        CommentManipulationHelper commentManipulationHelper,
        IPredictionDatabaseService predictionDatabaseService
        )
    {
        _logger = logger;
        _openAiSettings = openAiSettings.Value;
        _signalRService = signalRService;
        _commentManipulationHelper = commentManipulationHelper;
        _predictionDatabaseService = predictionDatabaseService;
    }
    
    public async Task<ChatChoice> SendOpenAiCompletion(IDbPrediction dbPrediction)
    {
        ChatCompletionsOptions options = CreateChatCompletionsOptions(dbPrediction.Prompt);
        OpenAIClient openAiClient = new OpenAIClient(_openAiSettings.ApiToken);

        try
        {
            var response = await openAiClient.GetChatCompletionsAsync(_openAiSettings.ModelName, options);
            _logger.LogDebug("LLM usage for {fileName} was {usage}", dbPrediction.FileName, response.Value.Usage);
            return response.Value.Choices.First();
        }
        catch (Exception e)
        {
            await _signalRService.InvokeErrorMessage(dbPrediction.ClientConnectionId, "The connection with the OpenAI API could not be established.");
            throw;
        }
    }

    public void ProcessApiResponse(ChatChoice openAiResponse, IDbPrediction dbPrediction)
    {
        string codeWithComments = 
            _commentManipulationHelper.ReplaceCommentInCode(openAiResponse.Message.Content, dbPrediction.InputCode);
        
        
        _predictionDatabaseService.UpdatePredictionResponseText(dbPrediction, openAiResponse.Message.Content);
        _predictionDatabaseService.UpdatePredictionEditedResponseText(dbPrediction, codeWithComments);
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