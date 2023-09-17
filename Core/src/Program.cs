using aia_api.Application.Azure;
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
app.MapPost("/uploadZip", ZipUploadHandler.UploadZipHandler(fileHandlerStreet))
    .AddEndpointFilter<EmptyFileFilter>();

app.MapPost("/upload", async (IFormFile compressedFile, HttpContext context, AzureClient client) =>
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
        await client.Pipeline(memStream, compressedFile.FileName);
    }

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("File successfully received.");
}).AddEndpointFilter<EmptyFileFilter>();



app.Run();
