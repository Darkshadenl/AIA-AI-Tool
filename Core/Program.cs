using aia_api.Application.Azure;
using aia_api.Application.EndpointFilter;
using aia_api.Application.FileHandler;
using aia_api.Configuration;
using Azure.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

var fileHandlerStreet = new ZipHandlerInMemory();

var supportedContentTypes = new[] { "application/zip" };

app.MapPost("/upload", async (IFormFile compressedFile, HttpContext context) =>
{
    if (!supportedContentTypes.Contains(compressedFile.ContentType))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid file type. Only ZIP files are allowed.");
        return;
    }

    var memoryStream = new MemoryStream();
    await compressedFile.CopyToAsync(memoryStream);
    memoryStream.Position = 0;
    var filteredResult = fileHandlerStreet.Handle(memoryStream, compressedFile.ContentType);

    await using (var memStream = filteredResult.Result)
    {
        memStream.Position = 0;
        // var azureApi = client;
        // await azureApi.Pipeline(memStream, compressedFile.FileName);
    }

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("File successfully received.");
}).AddEndpointFilter<EmptyFileFilter>();



app.Run();
