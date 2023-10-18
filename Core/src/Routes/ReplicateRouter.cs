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
        return (id, context, fileSystemStorageService, settings, resultDto, db) => {
            if (resultDto.status == "succeeded")
            {
                Console.WriteLine("Incoming LLM data for id: " + id);
                
                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);
                
                int codeStartIndex = Array.IndexOf(resultDto.output, "```");
                if (codeStartIndex <= -1) throw new IndexOutOfRangeException();
                
                var result = CombineTokens(resultDto.output);
                
                string fileName = GetFileName(result);
                string fileExtension = GetFileExtension(fileName);
                result = result.Replace(fileName, "");
                
                if (settings.Value.AllowedFileTypes.Contains(fileExtension))
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
                
                    GenerateFile(fileName, result, fileSystemStorageService);
                
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

    private static void GenerateFile(string fileName, string content, IFileSystemStorageService fileSystemStorageService)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(content);
        var memoryStream = new MemoryStream(byteArray);
        fileSystemStorageService.StoreInTemp(memoryStream, fileName);
    }

    private static string GetFileName(string result)
    {
        return Regex.Match(result, @"\w+\.\w+$", RegexOptions.Multiline).Value;
    }

    private static string GetFileExtension(string fileName)
    {
        return Regex.Match(fileName, @"\.\w+$", RegexOptions.Multiline).Value;
    }
}
