using System.IO.Abstractions;
using System.Net;
using System.Text;
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

    public static Func<int, HttpContext, IFileSystemStorageService, IOptions<Settings>, ReplicateCodeLlamaResultDTO, 
        PredictionDbContext, IServiceBusService, Task> ReplicateWebhook()
    {
        return (id, context, fileSystemStorageService, settings, resultDto, db, serviceBusService) => {
            if (resultDto.status == "succeeded")
            {
                Console.WriteLine("Incoming LLM data for id: " + id);
                
                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);
                
                // string result = CombineTokens(resultDto.output);
                string result = """
                                // Calculates the sum of two numbers
                                function calculateSum(number1, number2) {
                                    return number1 + number2;
                                }
                                
                                calculateSum(5, 5);
                                """;

                if (settings.Value.AllowedFileTypes.Contains(dbPrediction.FileExtension))
                {
                    try
                    {
                        dbPrediction.PredictionResponseText = result;
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

        string contentType;
        if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType))
        {
            contentType = "";
            Console.WriteLine("Could not find content type of file {0}.", fileName);
        }

        await connection.InvokeAsync("ReturnLLMResponse", fileName, contentType, content);
    }
}
