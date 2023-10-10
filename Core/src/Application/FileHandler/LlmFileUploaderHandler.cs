using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.Extensions.Options;

namespace aia_api.Application.FileHandler;

public class LlmFileUploaderHandler : AbstractFileHandler
{
    private readonly ReplicateApi _replicateApi;
    private readonly ReplicateSettings _replicateSettings;

    public LlmFileUploaderHandler(IOptions<Settings> settings,
        IOptions<ReplicateSettings> replicateSettings, ReplicateApi replicateApi) : base(settings)
    {
        _replicateApi = replicateApi;
        _replicateSettings = replicateSettings.Value;
    }

    public override async Task<IHandlerResult> Handle(string inputPath, string inputContentType)
    {
        var prediction = new Prediction(
            version: _replicateSettings.ModelVersion,
            input: new PredictionInput(
                prompt: _replicateSettings.Prompt,
                // SystemPrompt: _replicateSettings.SystemPrompt,
                max_tokens: 500,
                temperature: 0.8,
                top_p: 0.95,
                top_k: 10,
                frequency_penalty: 0,
                presence_penalty: 0,
                repeat_penalty: 1.1
            ),
            webhook: _replicateSettings.WebhookUrl
            // webhook_events_filter: _replicateSettings.WebhookFilters
        );

        var response = await _replicateApi.RunPrediction(prediction);

        return new HandlerResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            ErrorMessage = response.ReasonPhrase ?? string.Empty
        };
    }

}
