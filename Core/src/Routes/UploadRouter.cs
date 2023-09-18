using aia_api.Application.Azure;
using aia_api.Application.FileHandler;
using aia_api.Application.Gitlab;
using aia_api.Routes.DTO;

namespace aia_api.Routes;

public class UploadRouter
{
    private static string[] _supportedContentTypes = new[] { "application/zip" };

    public static Func<IFormFile, HttpContext, AzureClient, IFileHandlerFactory, Task> ZipHandler()
    {
        return async (IFormFile compressedFile, HttpContext context,
            AzureClient client, IFileHandlerFactory fileHandlerFactory) =>
        {
            if (!_supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var handlerStreet = fileHandlerFactory.GetFileHandler();
            var memoryStream = new MemoryStream();
            await compressedFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var filteredResult = handlerStreet.Handle(memoryStream, compressedFile.ContentType);

            await using (var memStream = filteredResult.Result)
            {
                memStream.Position = 0;
                await client.Pipeline(memStream, compressedFile.FileName);
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("File successfully received.");
        };
    }

    public static Func<UploadRepoDTO, HttpContext, HttpClient, Task> RepoHandler()
    {
        return async (UploadRepoDTO dto, HttpContext context, HttpClient httpClient) =>
        {
            var projectId = dto.projectId;
            var url = $"https://gitlab.com/api/v4/projects/{projectId}/repository/archive.zip";
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "TempDownloads");
            var fileName = $"{dto.projectId}.zip";
            var fullPath = Path.Combine(directoryPath, fileName);

            if (projectId.Length == 0 || string.IsNullOrWhiteSpace(projectId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid project id. Please provide a valid projectid.");
                return;
            }

            if (dto.apiToken.Length == 0 || string.IsNullOrWhiteSpace(dto.apiToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid api token. Please provide a valid api token.");
                return;
            }

            using var downloadClient = httpClient;
            downloadClient.DefaultRequestHeaders.Add("Private-Token", dto.apiToken);

            using var response = await downloadClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid project id. Please provide a valid projectid.");
                return;
            }

            try
            {
                await using var fileStream =
                    new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fileStream);
                Console.WriteLine("File downloaded.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("File successfully received.");
        };
    }
}
