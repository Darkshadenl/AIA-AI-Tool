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

app.MapPost("/api/upload/zip", UploadRouter.ZipHandler())
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());


app.MapGet("/api/replicate-webhook-test", ReplicateRouter.ReplicateWebhookTest());

app.MapPost("/api/replicate-webhook", ReplicateRouter.ReplicateWebhook());

app.MapGet("/api/health", () => Results.Ok("OK"));

app.MapGet("/api/db-test", (PredictionDbContext dbContext) =>
{
    var prediction = new DbPrediction
    {
        FileExtension = ".ts",
        FileName = "test.ts",
        Prompt = "Show me 5 cat jokes",
        PredictionResponseText = "This is a cat joke"
    };

    dbContext.Add(prediction);

    dbContext.SaveChanges();

    Console.WriteLine(prediction.Id);

    Results.Ok("OK");
});


app.Run();





