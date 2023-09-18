using System.IO.Compression;
using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class GZipHandlerInMemory : AbstractFileHandler
{
    private const string ContentType = "application/gzip";

    public GZipHandlerInMemory(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }

    public async Task<MemoryStream> Handle(MemoryStream input, string contentType)
    {
        if (!IsValidFile(input, contentType, ContentType))
            throw new NotSupportedException("Invalid file type. Only ZIP files are allowed.");

        // using var archive = InitializeInputArchive(input);
        // var outputMemoryStream = new MemoryStream();
        // using var outputArchive = InitializeOutputArchive(outputMemoryStream);

        await using var gzipStream = new GZipStream(input, CompressionMode.Decompress);
        var outputMemoryStream = new MemoryStream();

        await gzipStream.CopyToAsync(outputMemoryStream);


        // await ProcessEntries(archive, outputArchive);

        // LogExtensionsCount();

        return outputMemoryStream;

    }

    public void SetNext(IUploadedFileHandler next)
    {
        Next = next;
    }

    // private GZipStream InitializeInputArchive(MemoryStream zipMemoryStream)
    // {
    //     return new ZipArchive(zipMemoryStream, ZipArchiveMode.Read, true);
    // }
    //
    // private GZipStream InitializeOutputArchive(MemoryStream outputMemoryStream)
    // {
    //     return new ZipArchive(outputMemoryStream, ZipArchiveMode.Create, true);
    // }

    // private async Task ProcessEntries(MemoryStream archive, ZipArchive outputArchive)
    // {
    //     foreach (var entry in archive.Entries)
    //     {
    //         if (string.IsNullOrEmpty(GetExtension(entry))) continue;
    //
    //         var extension = GetExtension(entry);
    //
    //         CountExtension(extension);
    //
    //         if (IsSupportedExtension(extension))
    //         {
    //             await CopyEntryToNewArchive(entry, outputArchive);
    //         }
    //     }
    // }

    private string GetExtension(ZipArchiveEntry entry)
    {
        return Path.GetExtension(entry.FullName);
    }

    private void CountExtension(string extension)
    {
        if (!ExtensionsCount.ContainsKey(extension))
            ExtensionsCount[extension] = 1;
        else
            ExtensionsCount[extension]++;
    }


    private bool IsSupportedExtension(string extension)
    {
        return new[] { ".py", ".cs", ".ts", ".js" }.Contains(extension);
    }

    // private async Task CopyEntryToNewArchive(GZipStream entry, GZipStream outputArchive)
    // {
    //     var newEntry = outputArchive.CreateEntry(entry.FullName);
    //     await using var originalStream = entry.Open();
    //     await using var newStream = newEntry.Open();
    //     await originalStream.CopyToAsync(newStream);
    // }

    private void LogExtensionsCount()
    {
        foreach (var (key, value) in ExtensionsCount)
            Console.WriteLine($"{key}: {value}");
    }

}
