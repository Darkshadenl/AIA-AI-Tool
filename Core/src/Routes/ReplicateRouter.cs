using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Routes.DTO;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Routes;

public class ReplicateRouter
{
    public static Func<ReplicateApi, IOptions<Settings>, IOptions<ReplicateSettings>, IFileSystemStorageService, Task<IResult>> ReplicateWebhookTest()
    {
        return async (ReplicateApi replicateApi, IOptions<Settings> extensionSettings,
            IOptions<ReplicateSettings> replicateSettings,  IFileSystemStorageService storageService) => {

            Console.WriteLine("replicate-webhook-test");

            var llm = new LLMFileUploaderHandler(extensionSettings, storageService, replicateSettings, replicateApi);
            await llm.Handle("test", "test");

            return Results.Ok("replicate-webhook-test");
        };
    }

    public static Func<HttpContext, ReplicateResultDTO, Task> ReplicateWebhook()
    {
        return async (HttpContext context, ReplicateResultDTO resultDto) => {
            if (resultDto.Status == "succeeded")
            {
                Console.WriteLine("succeeded");
            }
            context.Response.StatusCode = 204;
        };
    }
}
