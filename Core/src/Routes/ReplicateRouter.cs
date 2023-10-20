using System.IO.Abstractions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Routes.DTO;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;

namespace aia_api.Routes;

public class ReplicateRouter
{
    [Obsolete("Remove at a later stage after implementing ReplicateWebHook further")]
    public static Func<ReplicateApi, IOptions<Settings>, IOptions<ReplicateSettings>, IFileSystem, IPredictionDatabaseService, Task<IResult>> ReplicateWebhookTest()
    {
        return async (replicateApi, settings, replicateSettings, fs, predictionDatabaseService) => {

            Console.WriteLine("replicate-webhook-test");

            var llm = new LlmFileUploaderHandler(settings, replicateSettings, replicateApi, fs, predictionDatabaseService);

            var inputPath = settings.Value.TempFolderPath + "/joost-main.zip";

            await llm.Handle(inputPath, "zip");

            return Results.Ok("replicate-webhook-test");
        };
    }

    public static Func<int, HttpContext, IOptions<Settings>, ReplicateCodeLlamaResultDTO, 
        PredictionDbContext, IServiceBusService, Task> ReplicateWebhook()
    {
        return (id, context, settings, resultDto, db, serviceBusService) => {
            if (resultDto.status == "succeeded")
            {
                Console.WriteLine("Incoming LLM data for id: " + id);
                
                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);
                
                string result = CombineTokens(resultDto.output);
                string[] codeLines = dbPrediction.InputCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                string resultWithComments = AddCommentsToCode(codeLines, result);

                if (settings.Value.AllowedFileTypes.Contains(dbPrediction.FileExtension))
                {
                    try
                    {
                        dbPrediction.PredictionResponseText = resultWithComments;
                        db.Entry(dbPrediction).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                
                    SendLlmResponseToFrontend(dbPrediction.FileName, result, serviceBusService);
                
                    Console.WriteLine("succeeded");
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return Task.CompletedTask;
        };
    }

    private static string AddCommentsToCode(string[] lines, string llmResponse)
    {
        string[] comments = GetComments(llmResponse);

        foreach (string comment in comments)
        {
            int lineNumber = GetLineNumber(comment);
            if (lineNumber >= 0 && lineNumber < lines.Length)
            {
                string inlineComment = comment.Replace($"{lineNumber} - ", "//");
                lines[lineNumber] = inlineComment + "\n" + lines[lineNumber];
            }
        }
        
        return string.Join(Environment.NewLine, lines);
    }

    private static string[] GetComments(string comments)
    {
         return comments.Split("line ");
    }

    private static int GetLineNumber(string comment)
    {
        string match = Regex.Match(comment, @"^\d+", RegexOptions.Multiline).Value;
        if (int.TryParse(match, out int lineNumber))
            return lineNumber;
        return -1;
    }

    private static string CombineTokens(string[] tokens)
    {
        var stringBuilder = new StringBuilder();
        foreach (string token in tokens)
            stringBuilder.Append(token);
        return stringBuilder.ToString().Trim();

    }

    private static async void SendLlmResponseToFrontend(string fileName, string content, IServiceBusService serviceBusService)
    {
        HubConnection connection = serviceBusService.GetConnection();
        if (connection.State != HubConnectionState.Connected) return;

        if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
        {
            contentType = "";
            Console.WriteLine("Could not find content type of file {0}.", fileName);
        }

        await connection.InvokeAsync("ReturnLLMResponse", fileName, contentType, content);
    }
}
