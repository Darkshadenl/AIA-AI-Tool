using aia_api.Application.Azure;
using aia_api.Application.FileHandler;
using aia_api.Application.Gitlab;
using aia_api.Routes.DTO;

namespace aia_api.Routes;

public class UploadRouter
{
    private static string[] _supportedContentTypes = new[] { "application/zip" };

    public static Func<IFormFile, HttpContext, AzureClient, Task> ZipHandler(ZipHandlerInMemory zipHandlerInMemory)
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

    public static Func<UploadRepoDTO, HttpContext, GitlabApi, Task> RepoHandler()
    {
        return async (UploadRepoDTO dto, HttpContext context, GitlabApi gitlabApi) =>
        {
            var url = dto.HttpsRepoUrl;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid url. Please provide a valid url.");
                return;
            }

            if (dto.apiToken.Length == 0 || string.IsNullOrWhiteSpace(dto.apiToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid api token. Please provide a valid api token.");
                return;
            }

            gitlabApi.DownloadRepo(dto.HttpsRepoUrl, dto.apiToken);

            await Task.CompletedTask;
        };
    }
}
