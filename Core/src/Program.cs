using aia_api.Application.EndpointFilter;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration;
using aia_api.Configuration.Records;
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


app.MapGet("/api/replicate-webhook-test", async (ReplicateApi replicateApi, IOptions<Settings> extensionSettings,
    IOptions<ReplicateSettings> replicateSettings,  IFileSystemStorageService storageService) => {

    Console.WriteLine("replicate-webhook-test");

    var llm = new LLMFileUploaderHandler(extensionSettings, storageService, replicateSettings, replicateApi);
    await llm.Handle("test", "test");

    return Results.Ok("replicate-webhook-test");
});

app.MapPost("/api/replicate-webhook", (HttpContext context, ReplicateResultDTO resultDto) => {
    if (resultDto.Status == "succeeded")
    {
        Console.WriteLine("succeeded");
    }
});

app.MapGet("/api/health", () => Results.Ok("OK"));


app.Run();

