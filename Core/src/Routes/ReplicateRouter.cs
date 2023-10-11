using System.IO.Abstractions;
using System.Net;
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
    public static Func<ReplicateApi, IOptions<Settings>, IOptions<ReplicateSettings>, IFileSystem, PredictionDbContext, Task<IResult>> ReplicateWebhookTest()
    {
        return async (replicateApi, settings, replicateSettings, fs, dbContext) => {

            Console.WriteLine("replicate-webhook-test");

            var llm = new LlmFileUploaderHandler(settings, replicateSettings, replicateApi, fs, dbContext);

            var inputPath = settings.Value.TempFolderPath + "/joost-main.zip";

            await llm.Handle(inputPath, "zip");

            return Results.Ok("replicate-webhook-test");
        };
    }

    public static Func<int, HttpContext, ReplicateResultDTO, PredictionDbContext, Task> ReplicateWebhook()
    {
        return (id, context, resultDto, db) => {
            if (resultDto.status == "succeeded")
            {
                Console.WriteLine("Incoming LLM data for id: " + id);

                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);

                var mergedOutput = new StringBuilder();
                foreach (var element in resultDto.output)
                    mergedOutput.Append(element);

                try
                {
                    var mergedText = mergedOutput.ToString().Trim();
                    dbPrediction.PredictionResponseText = mergedText;
                    db.Entry(dbPrediction).State = EntityState.Modified;
                    var written = db.SaveChanges();
                    Console.WriteLine(written + " record written to database");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return Task.CompletedTask;
        };
    }
}
