namespace aia_api.Application.Helpers;

public class FilesystemHelpers
{
    public static string GenerateFilePathWithDate(string filename = "", string path = "Temp", string extension = "zip")
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss").Replace(" ", "_");
        var fileName = $"{filename}_{date}.{extension}";

        return Path.Combine(directoryPath, fileName);
    }
}
