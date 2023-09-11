var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/uploadzip", async (IFormFile zipFile, HttpContext context) =>
{
    if (zipFile is null || zipFile.Length == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("No file received or file is empty.");
        return;
    }

    if (zipFile.ContentType != "application/zip" && zipFile.ContentType != "application/gzip")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid file type. Only ZIP and tar.gz files are allowed.");
        return;
    }

    // Hier kun je de logica toevoegen om het ZIP-bestand te verwerken
    // Bijvoorbeeld: Opslaan in Azure Blob Storage, uitpakken, enz.

    context.Response.StatusCode = 200;
    await context.Response.WriteAsync("ZIP file successfully received.");
});



app.Run();
