using aia_api.Application.EndpointFilter;
using aia_api.Configuration;
using aia_api.Routes;
using aia_api.src.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapPost("/api/upload/zip", UploadRouter.ZipHandler())
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());

app.MapHub<FileHub>("/api/upload/zip");

app.Run();

