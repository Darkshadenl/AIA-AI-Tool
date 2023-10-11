using System.Text;
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

    public static Func<HttpContext, IFileSystemStorageService, IOptions<Settings>, ReplicateResultDTO, Task> ReplicateWebhook()
    {
        return async (context, fileSystemStorageService, settings, resultDto) => {
            if (resultDto.Status == "succeeded")
            {
                string fileExtension = ReceiveFileType(settings.Value.AllowedFiles, resultDto.Output);
                int codeStartIndex = Array.IndexOf(resultDto.Output, "```");
                if (fileExtension == "" || codeStartIndex <= -1) return;
                
                string result = CombineTokens(resultDto.Output, codeStartIndex);
                Console.WriteLine(result);
                
                GenerateFile(fileExtension, result, fileSystemStorageService);
                
                Console.WriteLine("succeeded");
            }
            context.Response.StatusCode = 204;
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

    private static string CombineTokens(string[] tokens, int startIndex)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = startIndex + 1; i < tokens.Length - 1; i++)
        {
            stringBuilder.Append(tokens[i]);
        }

        return stringBuilder.ToString();
    }

    private static void GenerateFile(string fileExtension, string content, IFileSystemStorageService fileSystemStorageService)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(content);
        var memoryStream = new MemoryStream(byteArray);
        fileSystemStorageService.StoreInTemp(memoryStream, "test." + fileExtension);
    }
}
