using aia_api.Application.Helpers.Factories;
using aia_api.Routes.DTO;
using aia_api.Services;
using InterfacesAia;

namespace aia_api.Routes;

public class UploadRouter
{
    public static Func<IFormFile, HttpContext, IFileHandlerFactory, IStorageService, Task> ZipHandler()
    {
        string[] supportedContentTypes =  { "application/zip" };

        return async (compressedFile, context, fileHandlerFactory, storageService) =>
        {
            if (!supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var fileName = compressedFile.FileName;
            var handlerStreet = fileHandlerFactory.GetFileHandler();

            try
            {
                Stream inputStream = compressedFile.OpenReadStream();
                var path = await storageService.StoreInTemp(inputStream, fileName);
                await handlerStreet.Handle(path, compressedFile.ContentType);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
                context.Response.StatusCode = 400;
                return;
            }
            context.Response.StatusCode = 204;
        };
    }

    public static  Func<UploadRepoDTO, HttpContext, GitlabService, IFileHandlerFactory, Task> RepoHandler()
    {
        return async (dto, context, gitlabApi, fileHandlerFactory) =>
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
                var downloadPath = await gitlabApi.DownloadRepository(projectId, apiToken);
                IUploadedFileHandler handlerStreet = fileHandlerFactory.GetFileHandler();
                await handlerStreet.Handle(downloadPath, "application/zip");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
                context.Response.StatusCode = 400;
                return;
            }

            context.Response.StatusCode = 204;
        };
    }
}
