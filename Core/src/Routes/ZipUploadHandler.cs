using aia_api.Application.Azure;
using aia_api.Application.FileHandler;

namespace aia_api.Routes;

public class ZipUploadHandler
{
    private static string[] _supportedContentTypes = new[] { "application/zip" };

    public static Func<IFormFile, HttpContext, AzureClient, Task> UploadZipHandler(ZipHandlerInMemory zipHandlerInMemory)
    {
        return async (IFormFile compressedFile, HttpContext context, AzureClient client) =>
        {
            if (!_supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var memoryStream = new MemoryStream();
            await compressedFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var filteredResult = zipHandlerInMemory.Handle(memoryStream, compressedFile.ContentType);

            await using (var memStream = filteredResult.Result)
            {
                memStream.Position = 0;
                await client.Pipeline(memStream, compressedFile.FileName);
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("File successfully received.");
        };
    }
}
