using aia_api.Application.EndpointFilter;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration;
using aia_api.Configuration.Records;
using aia_api.Database;
using aia_api.Routes;
using aia_api.Routes.DTO;
using InterfacesAia;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

var api = app.MapGroup("/api");
var db = app.MapGroup("/db");

api.MapPost("/upload/zip", UploadRouter.ZipHandler())
    .AddEndpointFilter<EmptyFileFilter>();

api.MapPost("/upload/repo", UploadRouter.RepoHandler());

api.MapGet("/replicate-webhook-test", ReplicateRouter.ReplicateWebhookTest());

api.MapPost("/replicate-webhook/{id}", ReplicateRouter.ReplicateWebhook());

api.MapGet("/health", () => Results.Ok("OK"));

db.MapDelete("/clear-db", (PredictionDbContext dbContext) =>
{
    var entitiesToRemove = dbContext.Predictions.ToList();

    foreach (var entity in entitiesToRemove)
        dbContext.Remove(entity);

    dbContext.SaveChanges();

    return Results.Ok("Database cleared successfully.");
});



app.Run();





