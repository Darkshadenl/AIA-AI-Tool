using System.IO.Abstractions;
using System.IO.Compression;
using aia_api.Application.Helpers;
using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler
{
    public class ZipHandler : AbstractFileHandler
    {
        private readonly IOptions<Settings> _settings;
        private readonly IFileSystem _fileSystem;
        private const string ContentType = "application/zip";

        public ZipHandler(IOptions<Settings> settings, IFileSystem fileSystem) : base(settings)
        {
            _settings = settings;
            _fileSystem = fileSystem;
        }

        public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
        {
            if (!IsValidFile(inputContentType, ContentType))
            {
                if (Next != null)
                    return await Next.Handle(inputPath,  inputContentType);
                return await base.Handle(inputPath, inputContentType);
            }

            using ZipArchive archive = InitializeInputArchive(inputPath);
            using ZipArchive outputArchive =
                InitializeOutputArchive(Path.Combine(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath)));

            await ProcessEntries(archive, outputArchive);

            if (EnvHelper.IsDev())
                LogExtensionsCount();

            outputArchive.Dispose();
            archive.Dispose();

            if (Next == null)
                return await base.Handle(inputPath, inputContentType);
            return await Next.Handle(inputPath, inputContentType);
        }

        private ZipArchive InitializeInputArchive(string path)
        {
            var fileStream = _fileSystem.FileStream.New(path, FileMode.Open, FileAccess.Read);
            return new ZipArchive(fileStream, ZipArchiveMode.Read);
        }

        private ZipArchive InitializeOutputArchive(string outputPath)
        {
            if (!_fileSystem.Directory.Exists(_settings.Value.OutputFolderPath))
                _fileSystem.Directory.CreateDirectory(_settings.Value.OutputFolderPath);

            var fs = _fileSystem.FileStream.New(outputPath, FileMode.Create, FileAccess.Write);
            return new ZipArchive(fs, ZipArchiveMode.Create);
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
