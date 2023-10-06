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


app.MapGet("/api/replicate-webhook-test", ReplicateRouter.ReplicateWebhookTest());

app.MapPost("/api/replicate-webhook", ReplicateRouter.ReplicateWebhook());

app.MapGet("/api/health", () => Results.Ok("OK"));


app.Run();





