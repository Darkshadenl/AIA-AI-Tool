using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Routes.DTO;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Routes;

public class ReplicateRouter
{
    [Obsolete("Remove at a later stage after implementing ReplicateWebHook further")]
    public static Func<ReplicateApi, IOptions<Settings>, IOptions<ReplicateSettings>, Task<IResult>> ReplicateWebhookTest()
    {
        return async (replicateApi, extensionSettings, replicateSettings) => {

            Console.WriteLine("replicate-webhook-test");

            var llm = new LlmFileUploaderHandler(extensionSettings, replicateSettings, replicateApi);
            await llm.Handle("test", "test");

            return Results.Ok("replicate-webhook-test");
        };
    }

    public static Func<HttpContext, ReplicateResultDTO, Task> ReplicateWebhook()
    {
        return async (context, resultDto) => {
            if (resultDto.Status == "succeeded")
            {
                Console.WriteLine("succeeded");
            }
            context.Response.StatusCode = 204;
        };
    }
}
