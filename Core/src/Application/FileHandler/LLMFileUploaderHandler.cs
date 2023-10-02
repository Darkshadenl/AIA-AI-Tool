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

    public LLMFileUploaderHandler(IOptions<Settings> settings, ReplicateSettings replicateSettings) : base(settings)
    {
        _settings = settings;
        _replicateSettings = replicateSettings;
    }

    public override async Task Handle(string inputPath, string inputContentType)
    {
        var outputPath = Path.Combine(_settings.Value.OutputFolderPath, Path.GetFileName(inputPath));

        var prediction = new Prediction(
            ModelVersion: _replicateSettings.ModelVersion,
            ReplicateUrl: _replicateSettings.ReplicateUrl,
            PredictionInput: new PredictionInput(
                Prompt: _replicateSettings.Prompt,
                SystemPrompt: _replicateSettings.SystemPrompt,
                MaxTokens: 500,
                Temperature: 0.8,
                TopP: 0.95,
                TopK: 10,
                FrequencyPenalty: 0,
                PresencePenalty: 0,
                RepeatPenalty: 1.1
            ),
            WebhookUrl: _replicateSettings.WebhookUrl,
            WebhookFilter: _replicateSettings.WebhookFilters
        );

        if (Next == null)
        {

        }
        else
        {
            await Next.Handle(inputPath, inputContentType);
        }

    }

}
