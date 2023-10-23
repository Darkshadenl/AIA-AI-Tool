using System.Text;
using aia_api.Database;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;

namespace aia_api.Application.Controllers;

public class LlmResponseController
{
    private readonly ILogger<LlmResponseController> _logger;
    private readonly IServiceBusService _serviceBusService;

    public LlmResponseController(ILogger<LlmResponseController> logger, IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }

    public async void SendLlmResponseToFrontend(DbPrediction dbPrediction, string content)
    {
        HubConnection connection = _serviceBusService.GetConnection();
        if (connection.State != HubConnectionState.Connected) return;

        await connection.InvokeAsync("ReturnLLMResponse", dbPrediction.FileName, dbPrediction.FileExtension, content);
        _logger.LogInformation("File {fileName} send to clients", dbPrediction.FileName);
    }

    public string CombineTokens(string[] tokens)
    {
        var stringBuilder = new StringBuilder();
        foreach (string token in tokens)
            stringBuilder.Append(token);
        return stringBuilder.ToString().Trim();
    }

    public string ReplaceCommentInCode(string generatedComment, string content)
    {
        List<string> commentLines = generatedComment.Split(Environment.NewLine).ToList();
        List<string> contentLines = content.Split(Environment.NewLine).ToList();

        //Remove old comments
        int removeIndex = GetIndex(commentLines, contentLines);
        if (removeIndex < 0) return content;
        contentLines = RemoveOldComment(contentLines, removeIndex);
        
        //Insert new comments.
        int insertIndex = GetIndex(commentLines, contentLines);
        if (insertIndex < 0) return content;
        commentLines.Remove(commentLines.Last());
        contentLines.InsertRange(insertIndex, commentLines);

        return string.Join(Environment.NewLine, contentLines);
    }

    private List<string> RemoveOldComment(List<string> contentLines, int methodIndex)
    {
        int index = methodIndex - 1;
        while (contentLines[index].Trim().StartsWith("/"))
        {
            contentLines.Remove(contentLines[index]);
            index--;
            if (index < 0) break;
        }

        return contentLines;
    }

    private int GetIndex(List<string> commentLines, List<string> contentLines)
    {
        int index = contentLines.IndexOf(commentLines.Last());
        if (index < 0)
        {
            _logger.LogDebug("Could not find the location to place the comment in. Comment: {comment}, Code: {code}", 
                string.Join("", commentLines), string.Join("", contentLines));
        }

        return index;
    }
}