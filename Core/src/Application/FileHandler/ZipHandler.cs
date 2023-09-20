using System.IO.Compression;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler
{
    public class ZipHandler : AbstractFileHandler
    {
        private const string ContentType = "application/zip";

        public ZipHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
        { }

        public override async Task Handle(string inputPath, string outputPath, string inputContentType)
        {
            if (!IsValidFile(inputContentType, ContentType))
            {
                if (Next == null)
                    throw new Exception("No next handler found for this file type.");

                await Next.Handle(inputPath, outputPath, inputContentType);
                return;
            }

            using var archive = InitializeInputArchive(inputPath);
            using var outputArchive = InitializeOutputArchive(outputPath);

            await ProcessEntries(archive, outputArchive);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                // LogExtensionsCount();
            }
        }

        private ZipArchive InitializeInputArchive(string path)
        {
            return ZipFile.OpenRead(path);
        }

        private ZipArchive InitializeOutputArchive(string outputPath)
        {
            return ZipFile.Open(outputPath, ZipArchiveMode.Create);
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
}
