using System.IO.Abstractions;
using System.Net;
using aia_api.Application.Handlers.FileHandler;
using aia_api.Application.Helpers;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Routes.DTO;
using InterfacesAia.Services;
using Microsoft.Extensions.Options;

namespace aia_api.Routes;

public class ReplicateRouter
{
    [Obsolete("Remove at a later stage after implementing ReplicateWebHook further")]
    public static Func<ReplicateApi, ILogger<ReplicateRouter>, ILogger<LlmFileUploaderHandler>, IOptions<Settings>,
        IOptions<ReplicateSettings>, IFileSystem, IPredictionDatabaseService, Task<IResult>> ReplicateWebhookTest()
    {
        return async (replicateApi, logger, llmLogger, settings, replicateSettings, fs, predictionDatabaseService) => {
            logger.LogInformation("replicate-webhook-test");

            var llm = new LlmFileUploaderHandler(llmLogger, settings, replicateSettings, replicateApi, fs, predictionDatabaseService);

            var inputPath = settings.Value.TempFolderPath + "/joost-main.zip";

            await llm.Handle(inputPath, "zip");

            return Results.Ok("replicate-webhook-test");
        };
    }


    public static Func<int, HttpContext, ILogger<ReplicateRouter>, IOptions<Settings>, ReplicateCodeLlamaResultDTO,
        IPredictionDatabaseService, CommentManipulationHelper, ISignalRService, Task> ReplicateWebhook()
    {
        return (id, context, logger, settings, resultDto, databaseService, commentManipulationHelper, signalRService) => {
            if (resultDto.status == "succeeded")
            {
                logger.LogInformation("Incoming LLM data for id: {id}", id);

                var dbPrediction = databaseService.GetPrediction(id);

                if (settings.Value.AllowedFileTypes.Contains(dbPrediction.FileExtension))
                {

                    string result = commentManipulationHelper.CombineTokens(resultDto.output);
                    string codeWithComments = commentManipulationHelper.ReplaceCommentInCode(result, dbPrediction.InputCode);

                    databaseService.UpdatePrediction(dbPrediction, codeWithComments);
                    signalRService.SendLlmResponseToFrontend(dbPrediction.FileName, dbPrediction.FileExtension, codeWithComments);
                    logger.LogInformation("Llm response for {fileName} with id {id} was successfully processed", dbPrediction.Id, dbPrediction.FileName);
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return Task.CompletedTask;
        };
    }

}
