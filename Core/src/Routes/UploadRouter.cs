using aia_api.Application.FileHandler;
using aia_api.Application.Gitlab;
using aia_api.Application.Helpers;
using aia_api.Routes.DTO;
using InterfacesAia;

namespace aia_api.Routes;

public class UploadRouter
{
    private static readonly string FilteredOutputDirectory = Path.Combine("Temp", "Filtered");
    private static readonly string UnfilteredOutputDirectory = "Temp";

    public static Func<IFormFile, HttpContext, IFileHandlerFactory, Task> ZipHandler()
    {
        string[] supportedContentTypes =  { "application/zip" };

        return async (compressedFile, context, fileHandlerFactory) =>
        {
            if (!supportedContentTypes.Contains(compressedFile.ContentType))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
                return;
            }

            var fileName = compressedFile.FileName;
            var handlerStreet = fileHandlerFactory.GetFileHandler();
            var zipPath = FilesystemHelpers.GenerateFilePathWithDate(fileName, UnfilteredOutputDirectory);
            var filteredZipOutputPath = FilesystemHelpers.GenerateFilePathWithDate(fileName, FilteredOutputDirectory);

            try
            {
                Stream inputStream = compressedFile.OpenReadStream();
                Directory.CreateDirectory(Path.GetDirectoryName(zipPath));
                var fileStream = new FileStream(zipPath, FileMode.Create);
                await inputStream.CopyToAsync(fileStream);
                inputStream.Close();
                fileStream.Close();

                await handlerStreet.Handle(zipPath, filteredZipOutputPath, compressedFile.ContentType);
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

    public static  Func<UploadRepoDTO, HttpContext, GitlabApi, IFileHandlerFactory, Task> RepoHandler()
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
                var outputFilePath = FilesystemHelpers.GenerateFilePathWithDate(projectId, FilteredOutputDirectory);
                IUploadedFileHandler handlerStreet = fileHandlerFactory.GetFileHandler();
                await handlerStreet.Handle(downloadPath, outputFilePath, "application/zip");
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
