using aia_api.Configuration.Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace aia_api.Application.Azure;

public class AzureClient
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _blobContainerName;

    public AzureClient(BlobServiceClient blobClient, IOptions<AzureBlobStorageSettings> settings)
    {
        _blobContainerName = settings.Value.BlobContainerName;
        _blobServiceClient = blobClient;
    }

    public async Task ZipPipeline(string zipPath, string fileName)
    {
        await using var fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
        await UploadStreamToBlob(fileStream, fileName);
    }

    public async Task MemoryStreamPipeline(MemoryStream stream, string fileName)
    {
        stream.Position = 0;
        await UploadStreamToBlob(stream, fileName);
    }

    private async Task UploadStreamToBlob(Stream inputStream, string fileName)
    {
        var blobClient = CreateBlobClients(fileName);

        byte[] buffer = new byte[4 * 1024];
        await using var outputStream = await blobClient.OpenWriteAsync(true);
        int bytesRead;

        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await outputStream.WriteAsync(buffer, 0, bytesRead);
        }
    }

    private BlobClient CreateBlobClients(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        return blobClient;
    }
}
