using aia_api.Application.EndpointFilter;
using aia_api.Configuration;
using aia_api.Routes;
using Microsoft.AspNetCore.SignalR.Client;
using aia_api.src.Services;
using InterfacesAia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

IUploadController uploadController = app.Services.GetRequiredService<IUploadController>();

HubConnection connection = await ServiceBusService.ExecuteAsync();
connection.On<string, string, string>("UploadZip", uploadController.ZipHandler);

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());

app.Run();
