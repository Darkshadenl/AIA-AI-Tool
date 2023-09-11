using aia_api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/upload", async (IFormFile compressedFile, HttpContext context) =>
{
    if (compressedFile is null || compressedFile.Length == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("No file received or file is empty.");
        return;
    }

    if (compressedFile.ContentType != "application/zip" && compressedFile.ContentType != "application/gzip")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid file type. Only ZIP and tar.gz files are allowed.");
        return;
    }

    var handleCompressedFile = new HandleCompressedFile();

    var memoryStream = new MemoryStream();
    await compressedFile.CopyToAsync(memoryStream);
    memoryStream.Position = 0;

    switch (compressedFile.ContentType)
    {
        case "application/zip":
            handleCompressedFile.HandleZipFileInMemory(memoryStream);
            break;
        case "application/gzip":
            handleCompressedFile.HandleTarGzFileInMemory(compressedFile);
            break;
        default:
            throw new NotImplementedException();
    }

    // Hier kun je de logica toevoegen om het ZIP-bestand te verwerken
    // Bijvoorbeeld: Opslaan in Azure Blob Storage, uitpakken, enz.

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("File successfully received.");
});



app.Run();
