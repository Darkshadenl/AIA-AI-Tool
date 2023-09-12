using aia_api.Application.FileHandler;
using aia_api.Application.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var fileHandlerStreet = new ZipHandlerInMemory();
fileHandlerStreet.SetNext(new GZipHandlerInMemory());

var supportedContentTypes = new[] { "application/zip", "application/gzip" };

app.UseEmptyFileCheckMiddleware();

app.MapPost("/upload", async (IFormFile compressedFile, HttpContext context) =>
{
    if (!supportedContentTypes.Contains(compressedFile.ContentType))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid file type. Only ZIP and tar.gz files are allowed.");
        return;
    }

    var memoryStream = new MemoryStream();
    await compressedFile.CopyToAsync(memoryStream);
    memoryStream.Position = 0;
    var filteredResult = fileHandlerStreet.Handle(memoryStream, compressedFile.ContentType);

    // Hier kun je de logica toevoegen om het ZIP-bestand te verwerken
    // Bijvoorbeeld: Opslaan in Azure Blob Storage, uitpakken, enz.

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("File successfully received.");
});



app.Run();
