using aia_api.Application.EndpointFilter;
using aia_api.Configuration;
using aia_api.Routes;
using Microsoft.AspNetCore.SignalR.Client;
using InterfacesAia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

IUploadController uploadController = app.Services.GetRequiredService<IUploadController>();
IServiceBusService serviceBusService = app.Services.GetRequiredService<IServiceBusService>();

HubConnection connection = await serviceBusService.ExecuteAsync();
connection.On<string, string, byte[], int, int>("UploadChunk", uploadController.ReceiveFileChunk);

app.MapPost("/api/upload/zip", UploadRouter.ZipHandler())
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());


app.MapGet("/api/replicate-webhook-test", ReplicateRouter.ReplicateWebhookTest());

app.MapPost("/api/replicate-webhook", ReplicateRouter.ReplicateWebhook());

app.MapGet("/api/health", () => Results.Ok("OK"));


app.Run();
