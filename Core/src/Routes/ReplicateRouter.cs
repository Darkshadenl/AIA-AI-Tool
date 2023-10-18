using System.IO.Abstractions;
using System.Text;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Routes.DTO;
using InterfacesAia;
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

    public static Func<int, HttpContext, IFileSystemStorageService, IOptions<Settings>, ReplicateCodeLlamaResultDTO, PredictionDbContext, Task> ReplicateWebhook()
    {
        return async (id, context, fileSystemStorageService, settings, resultDto, db) => {
            if (resultDto.Status == "succeeded")
            {
                Console.WriteLine("Incoming LLM data for id: " + id);
                
                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);
                
                string fileExtension = ReceiveFileType(settings.Value.AllowedFiles, resultDto.Output);
                int codeStartIndex = Array.IndexOf(resultDto.Output, "```");
                if (fileExtension == "" || codeStartIndex <= -1) return;
                
                var result = CombineTokens(resultDto.Output);
                
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
                
                GenerateFile(fileExtension, result, fileSystemStorageService);
                
                Console.WriteLine("succeeded");
            }
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return Task.CompletedTask;
        };
    }

    private static string ReceiveFileType(string[] allowedFiles, string[] resultOutput)
    {
        foreach (var allowedFile in allowedFiles)
        {
            int fileExtensionIndex = Array.IndexOf(resultOutput, allowedFile.Substring(1));
            if (fileExtensionIndex >= 0) return resultOutput[fileExtensionIndex];
        }
        
        return "";
    }

    private static string CombineTokens(string[] resultOutput)
    {
        var mergedOutput = new StringBuilder();
        foreach (var element in resultOutput)
            mergedOutput.Append(element);
        return mergedOutput.ToString().Trim();
    }

    private static void GenerateFile(string fileExtension, string content, IFileSystemStorageService fileSystemStorageService)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(content);
        var memoryStream = new MemoryStream(byteArray);
        fileSystemStorageService.StoreInTemp(memoryStream, "test." + fileExtension);
    }
}
