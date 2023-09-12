using System.IO.Compression;
using InterfacesAia;

namespace aia_api.Application.FileHandler;

public class GZipHandlerInMemory : ICompressedFileHandler
{
    private ICompressedFileHandler _next;
    private Dictionary<string, int> _extensionsCount = new();
    private string _contentType = "\"application/gzip\"";

    public async Task<MemoryStream> Handle(MemoryStream input, string contentType)
    {
        // TODO Check filesize. If too large, do _next.Handle()

        if (contentType != _contentType) return await _next.Handle(input, contentType);

        using var archive = InitializeInputArchive(input);
        var outputMemoryStream = new MemoryStream();
        using var outputArchive = InitializeOutputArchive(outputMemoryStream);

        await ProcessEntries(archive, outputArchive);

        LogExtensionsCount();

        return outputMemoryStream;

    }

    public void SetNext(ICompressedFileHandler next)
    {
        _next = next;
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
            if (string.IsNullOrEmpty(GetExtension(entry))) continue;

            var extension = GetExtension(entry);

            CountExtension(extension);

            if (IsSupportedExtension(extension))
            {
                await CopyEntryToNewArchive(entry, outputArchive);
            }
        }
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
