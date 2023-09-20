using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

/// <summary>
/// If output directory does not exist, create it.
/// </summary>
public class PathCreationHandler  : AbstractFileHandler
{
    public PathCreationHandler(IOptions<Settings> extensionSettings) : base(extensionSettings)
    {
    }

    public override Task Handle(string inputPath, string outputPath, string inputContentType)
    {
        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        return Next.Handle(inputPath, outputPath, inputContentType);
    }
}
