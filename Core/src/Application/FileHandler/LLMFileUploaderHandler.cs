using System.IO.Abstractions;
using System.Text;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class LLMFileUploaderHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly Configuration.Records.ReplicateSettings _replicateSettings;

    public LLMFileUploaderHandler(IOptions<Settings> settings, IFileSystem fileSystem, ReplicateSettings replicateSettings) : base(settings)
    {
        _settings = settings;
        _replicateSettings = replicateSettings;
    }

    public override async Task Handle(string inputPath, string inputContentType)
    {
        var outputPath = Path.Combine(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));

        using (HttpClient httpClient = new HttpClient())
        {
            // Prepare your HTTP request payload
            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_replicateSettings.PredictionsUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Handle success
            }
            else
            {
                // Handle failure
            }
        }

        // Call the next handler in the chain if there is one
        if (Next == null)
        {

        }
        else
        {
            await Next.Handle(inputPath, inputContentType);
        }

    }

}
