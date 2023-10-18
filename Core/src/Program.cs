using aia_api.Application.EndpointFilter;
using aia_api.Configuration;
using aia_api.Database;
using aia_api.Routes;
using Microsoft.AspNetCore.SignalR.Client;
using InterfacesAia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.SetupDi(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

IUploadController uploadController = app.Services.GetRequiredService<IUploadController>();
IServiceBusService serviceBusService = app.Services.GetRequiredService<IServiceBusService>();
HubConnection connection = await serviceBusService.ExecuteAsync();

connection.On<string, string, byte[], int, int>("UploadChunk", uploadController.ReceiveFileChunk);

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
