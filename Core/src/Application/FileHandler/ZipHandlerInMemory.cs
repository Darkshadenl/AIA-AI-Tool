using System.IO.Compression;
using aia_api.Application.FileHandler.InputTypes;
using aia_api.Configuration.Azure;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class ZipHandlerInMemory : AbstractFileHandler
{
    private const string ContentType = "application/zip";

    public ZipHandlerInMemory(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
        HandlerType = FileDataType.MemoryStream;
    }

    public override async Task<MemoryStream> Handle(IInputData input, string inputContentType)
    {
        var inputFileDataType = GetFileDataType(input);
        if (!inputFileDataType.Equals(HandlerType))
            await Next.Handle(input, inputContentType);

        var memoryStreamFileData = (MemoryStreamFileData) input;
        var stream = memoryStreamFileData.Stream;

        if (!IsValidFile(stream, inputContentType, ContentType))
            throw new NotSupportedException("Invalid file type. Only ZIP files are allowed.");

        using var archive = InitializeInputArchive(stream);
        var outputMemoryStream = new MemoryStream();
        using var outputArchive = InitializeOutputArchive(outputMemoryStream);

        await ProcessEntries(archive, outputArchive);

        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            // LogExtensionsCount();
        }

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

    private async Task CopyEntryToNewArchive(ZipArchiveEntry entry, ZipArchive outputArchive)
    {
        var newEntry = outputArchive.CreateEntry(entry.FullName);
        await using var originalStream = entry.Open();
        await using var newStream = newEntry.Open();
        await originalStream.CopyToAsync(newStream);
    }
}
