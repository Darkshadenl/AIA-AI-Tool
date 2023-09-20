using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

/// <summary>
/// If output directory does not exist, create it.
/// </summary>
public class PathCreationHandler  : AbstractFileHandler
{
    public PathCreationHandler(IOptions<Settings> settings) : base(settings)
    {
    }

    public override Task Handle(string inputPath, string inputContentType)
    {
        // TODO after refactor does nothing anymore

        return Next.Handle(inputPath, inputContentType);
    }
}
