namespace aia_api;

using System.IO;
using System.IO.Compression;

public class HandleCompressedFile
{
    private Dictionary<string, int> _extensionsCount = new();

    public void HandleTarGzFileInMemory(IFormFile tarGzFile)
    {
        throw new NotImplementedException();
    }

    public async Task<MemoryStream> HandleZipFileInMemory(MemoryStream zipMemoryStream)
    {
        using var archive = InitializeInputArchive(zipMemoryStream);
        var outputMemoryStream = new MemoryStream();
        using var outputArchive = InitializeOutputArchive(outputMemoryStream);

        await ProcessEntries(archive, outputArchive);

        LogExtensionsCount();

        return outputMemoryStream;
    }

    private ZipArchive InitializeInputArchive(MemoryStream zipMemoryStream)
    {
        return new ZipArchive(zipMemoryStream, ZipArchiveMode.Read, true);
    }

    private ZipArchive InitializeOutputArchive(MemoryStream outputMemoryStream)
    {
        return new ZipArchive(outputMemoryStream, ZipArchiveMode.Create, true);
    }

    private async Task ProcessEntries(ZipArchive archive, ZipArchive outputArchive)
    {
        foreach (var entry in archive.Entries)
        {
            if (IsDirectory(entry)) continue;

            var extension = GetExtension(entry);

            CountExtension(extension);

            if (IsSupportedExtension(extension))
            {
                await CopyEntryToNewArchive(entry, outputArchive);
            }
        }
    }

    private bool IsDirectory(ZipArchiveEntry entry)
    {
        return string.IsNullOrEmpty(Path.GetExtension(entry.FullName));
    }

    private string GetExtension(ZipArchiveEntry entry)
    {
        return Path.GetExtension(entry.FullName);
    }

    private void CountExtension(string extension)
    {
        if (!_extensionsCount.ContainsKey(extension))
            _extensionsCount[extension] = 1;
        else
            _extensionsCount[extension]++;
    }

    private bool IsSupportedExtension(string extension)
    {
        return new[] { ".py", ".cs", ".ts", ".js" }.Contains(extension);
    }

    private async Task CopyEntryToNewArchive(ZipArchiveEntry entry, ZipArchive outputArchive)
    {
        var newEntry = outputArchive.CreateEntry(entry.FullName);
        await using var originalStream = entry.Open();
        await using var newStream = newEntry.Open();
        await originalStream.CopyToAsync(newStream);
    }

    private void LogExtensionsCount()
    {
        foreach (var (key, value) in _extensionsCount)
            Console.WriteLine($"{key}: {value}");
    }



}
