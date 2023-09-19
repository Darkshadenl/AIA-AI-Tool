namespace aia_api.Application.Helpers;

public class FilesystemHelpers
{
    public static string CreateZipFilePathWithDate(string filename = "", string path = "TempDownloads")
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss").Replace(" ", "_");
        var fileName = $"{filename}_{date}.zip";

        return Path.Combine(directoryPath, fileName);
    }
}
