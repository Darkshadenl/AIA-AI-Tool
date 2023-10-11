using System.IO.Abstractions;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Routes.DTO;
using InterfacesAia;
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

    public static Func<int, HttpContext, ReplicateResultDTO, Task> ReplicateWebhook()
    {
        return async (id, context, resultDto) => {
            if (resultDto.Status == "succeeded")
            {
                Console.WriteLine("succeeded");
                Console.WriteLine("id: " + id);
            }
            context.Response.StatusCode = 204;
        };
    }
}
