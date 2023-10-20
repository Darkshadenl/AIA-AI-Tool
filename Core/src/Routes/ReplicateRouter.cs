using System.IO.Abstractions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using aia_api.Application.Controllers;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Routes.DTO;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
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
        PredictionDbContext, IServiceBusService, LlmResponseController, Task> ReplicateWebhook()
    {
        return (id, context, logger, settings, resultDto, db, serviceBusService, llmResponseController) => {
            if (resultDto.status == "succeeded")
            {
                logger.LogInformation("Incoming LLM data for id: {id}", id);
                
                var dbPrediction = db.Predictions
                    .First(p => p.Id == id);
                
                string result = llmResponseController.CombineTokens(resultDto.output);

                if (settings.Value.AllowedFileTypes.Contains(dbPrediction.FileExtension))
                {
                    try
                    {
                        dbPrediction.PredictionResponseText = result;
                        db.Entry(dbPrediction).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical("Error: {message}, {stackTrace}", e.Message, e.StackTrace);
                        throw;
                    }
                
                    llmResponseController.SendLlmResponseToFrontend(dbPrediction, result, serviceBusService);
                    logger.LogInformation("Llm response successfully processed");
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            return Task.CompletedTask;
        };
    }
}
