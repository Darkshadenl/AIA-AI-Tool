using System.Net;
using aia_api.Routes.DTO;
using aia_api.Services;
using InterfacesAia;

namespace aia_api.Routes;

public class UploadRouter
{
    public static Func<IFormFile, HttpContext, IFileHandlerFactory, IFileSystemStorageService, Task> ZipHandler()
    {
        string[] supportedContentTypes =  { "application/zip" };

        return async (compressedFile, context, fileHandlerFactory, storageService) =>
        {
            if (!supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                Console.WriteLine("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var fileName = compressedFile.FileName;
            var handlerStreet = fileHandlerFactory.GetFileHandler();

            try
            {
                Stream inputStream = compressedFile.OpenReadStream();
                var path = await storageService.StoreInTemp(inputStream, fileName);
                var result = await handlerStreet.Handle(path, compressedFile.ContentType);

                context.Response.StatusCode = (int) result.StatusCode;

                if (!result.Success)
                    Console.WriteLine($"Error: {result.ErrorMessage}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
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
                Console.WriteLine("Invalid project id. Provide a valid projectid.");
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return;
            }

            if (apiToken.Length == 0 || string.IsNullOrWhiteSpace(apiToken))
            {
                Console.WriteLine("Invalid api token. Configure api token.");
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                var downloadPath = await gitlabApi.DownloadRepository(projectId, apiToken);
                IUploadedFileHandler handlerStreet = fileHandlerFactory.GetFileHandler();
                var result = await handlerStreet.Handle(downloadPath, "application/zip");

                context.Response.StatusCode = (int) result.StatusCode;

                if (!result.Success)
                    Console.WriteLine($"Error: {result.ErrorMessage}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                return;
            }

            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
        };
    }

}
