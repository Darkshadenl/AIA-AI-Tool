namespace aia_api;

using System.IO;
using System.IO.Compression;

public class HandleCompressedFile
{
    private Dictionary<string, int> extensionsCount = new Dictionary<string, int>();

    public void HandleTarGzFileInMemory(IFormFile tarGzFile)
    {
        throw new NotImplementedException();
    }

    public async void HandleZipFileInMemory(MemoryStream zipMemoryStream)
    {
        var extensionsCount = new Dictionary<string, int>();

        // Setup archive on a memorystream to read from zip
        using var archive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Read, true);

        // Setup memorystream for creating new zip
        using var outputMemoryStream = new MemoryStream();
        using var outputArchive = new ZipArchive(outputMemoryStream, ZipArchiveMode.Create, true);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(entry.FullName)))
                continue;

            var extension = Path.GetExtension(entry.FullName);


            if (!extensionsCount.ContainsKey(extension))
                extensionsCount[extension] = 1;
            else
                extensionsCount[extension]++;

            if (!new[] { ".py", ".cs", ".ts", ".js" }.Contains(extension)) continue;

            var newEntry = outputArchive.CreateEntry(entry.FullName);
            await using var originalStream = entry.Open();
            await using var newStream = newEntry.Open();
            await originalStream.CopyToAsync(newStream);
        }

        foreach (var (key, value) in extensionsCount)
            Console.WriteLine($"{key}: {value}");

    }



}
