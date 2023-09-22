using System.IO.Compression;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler
{
    public class ZipHandler : AbstractFileHandler
    {
        private readonly IOptions<Settings> _settings;
        private const string ContentType = "application/zip";

        public ZipHandler(IOptions<Settings> settings) : base(settings)
        {
            _settings = settings;
        }

        public override async Task Handle(string inputPath, string inputContentType)
        {
            if (!IsValidFile(inputContentType, ContentType))
            {
                if (Next == null)
                    throw new Exception("No next handler found for this file type.");

                await Next.Handle(inputPath,  inputContentType);
                return;
            }

            using ZipArchive archive = InitializeInputArchive(inputPath);
            using ZipArchive outputArchive =
                InitializeOutputArchive(Path.Combine(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath)));

            await ProcessEntries(archive, outputArchive);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                LogExtensionsCount();

            outputArchive.Dispose();
            archive.Dispose();

            await Next.Handle(inputPath, inputContentType);
        }

        private ZipArchive InitializeInputArchive(string path) =>
            ZipFile.OpenRead(path);

        private ZipArchive InitializeOutputArchive(string outputPath)
        {
            if (!Directory.Exists(_settings.Value.OutputFolderPath))
                Directory.CreateDirectory(_settings.Value.OutputFolderPath);

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

        private string GetExtension(ZipArchiveEntry entry) =>
            Path.GetExtension(entry.FullName);

        private async Task CopyEntryToNewArchive(ZipArchiveEntry entry, ZipArchive outputArchive)
        {
            var newEntry = outputArchive.CreateEntry(entry.FullName);
            await using var originalStream = entry.Open();
            await using var newStream = newEntry.Open();
            await originalStream.CopyToAsync(newStream);
        }
    }
}
