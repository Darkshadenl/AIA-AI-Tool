using System.IO.Abstractions;
using System.Text;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class LLMFileUploaderHandler : AbstractFileHandler
{
    private readonly IOptions<Settings> _settings;
    private readonly ReplicateSettings _replicateSettings;

    public LLMFileUploaderHandler(IOptions<Settings> settings, IFileSystem fileSystem, ReplicateSettings replicateSettings) : base(settings)
    {
        _settings = settings;
        _replicateSettings = replicateSettings;
    }

    public override async Task Handle(string inputPath, string inputContentType)
    {
        var outputPath = Path.Combine(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));

        var prediction = new Prediction(
            ModelVersion: _replicateSettings.ModelVersion,
            PredictionInput: new PredictionInput(
                Prompt: _replicateSettings.Prompt,
                SystemPrompt: _replicateSettings.SystemPrompt,
                MaxTokens: 64,
                Temperature: 0.0,
                TopP: 1.0,
                TopK: 0,
                FrequencyPenalty: 0.0,
                PresencePenalty: 0.0,
                RepeatPenalty: 0.0
            ),
            WebhookUrl: _replicateSettings.WebhookUrl,
            WebhookFilter: new string[] { "completed" }
        );

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
