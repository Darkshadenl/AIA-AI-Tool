using aia_api.Application.EndpointFilter;
using aia_api.Application.FileHandler;
using aia_api.Configuration;
using aia_api.Routes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectConfigs(builder.Configuration);
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

var fileHandlerStreet = new ZipHandlerInMemory();

var supportedContentTypes = new[] { "application/zip" };

/*
 * Example of extracting route functionality to separate file
 */
app.MapPost("/api/upload/zip", UploadRouter.ZipHandler(fileHandlerStreet))
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/api/upload/repo", UploadRouter.RepoHandler());


app.Run();
