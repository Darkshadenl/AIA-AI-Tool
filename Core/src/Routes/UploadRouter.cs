using aia_api.Application.Azure;
using aia_api.Application.FileHandler;
using aia_api.Application.FileHandler.InputTypes;
using aia_api.Application.Gitlab;
using aia_api.Routes.DTO;

namespace aia_api.Routes;

public class UploadRouter
{
    private static string[] _supportedContentTypes = { "application/zip" };

    public static Func<IFormFile, HttpContext, AzureClient, IFileHandlerFactory, Task> ZipHandler()
    {
        return async (compressedFile, context, client, fileHandlerFactory) =>
        {
            if (!_supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var handlerStreet = fileHandlerFactory.GetFileHandler();
            var inputMemoryStream = (MemoryStreamFileData) fileHandlerFactory.GetInputData(FileDataType.MemoryStream);
            inputMemoryStream.Stream = new MemoryStream();
            await compressedFile.CopyToAsync(inputMemoryStream.Stream);
            inputMemoryStream.Stream.Position = 0;
            var filteredResult = handlerStreet.Handle(inputMemoryStream, compressedFile.ContentType);

            await using (var memStream = filteredResult.Result)
            {
                memStream.Position = 0;
                await client.Pipeline(memStream, compressedFile.FileName);
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("File successfully received.");
        };
    }

    public static Func<UploadRepoDTO, HttpContext, GitlabApi, AzureClient, IFileHandlerFactory, Task> RepoHandler()
    {
        return async (dto, context, gitlabApi, client, fileHandlerFactory) =>
        {
            var projectId = dto.projectId;
            var apiToken = dto.apiToken;

            if (projectId.Length == 0 || string.IsNullOrWhiteSpace(projectId))
            {
                await context.Response.WriteAsync("Invalid project id. Please provide a valid projectid.");
                context.Response.StatusCode = 400;
                return;
            }

            if (apiToken.Length == 0 || string.IsNullOrWhiteSpace(apiToken))
            {
                await context.Response.WriteAsync("Invalid api token. Please provide a valid api token.");
                context.Response.StatusCode = 400;
                return;
            }

            try
            {
                var path = await gitlabApi.DownloadRepository(projectId, apiToken);

                // filter out files

                // Upload to Azure

            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Could not download repository.");
                return;
            }

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("File successfully received.");
        };
    }
}
