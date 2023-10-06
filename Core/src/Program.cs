using aia_api.Application.EndpointFilter;
using aia_api.Application.FileHandler;
using aia_api.Application.Replicate;
using aia_api.Configuration;
using aia_api.Configuration.Records;
using aia_api.Routes;
using InterfacesAia;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

app.MapPost("/api/upload/zip", UploadRouter.ZipHandler())
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());


app.MapGet("/api/llmHookTest", async (ReplicateApi replicateApi, IOptions<Settings> extensionSettings,
    IOptions<ReplicateSettings> replicateSettings,  IFileSystemStorageService storageService) => {
    Console.WriteLine("llmHookTest");

    var llm = new LLMFileUploaderHandler(extensionSettings, storageService, replicateSettings, replicateApi);
    await llm.Handle("test", "test");

    return Results.Ok("llmHookTest");
});

app.MapPost("/api/llmHook", (HttpContext context) => {
    System.Console.WriteLine("LLM Hook");
    Console.WriteLine(context.Request.Body);
});

app.MapGet("/api/health", () => Results.Ok("OK"));


app.Run();

