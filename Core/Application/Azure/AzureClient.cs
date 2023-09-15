using Azure.Core.Extensions;
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

    public async Task Pipeline(MemoryStream stream, string fileName)
    {
        var blobClient = CreateBlobClients(fileName);

        byte[] buffer = new byte[4 * 1024];
        stream.Position = 0;
        await using var outputStream = await blobClient.OpenWriteAsync(true);
        int bytesRead;

        try
        {
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await outputStream.WriteAsync(buffer, 0, bytesRead);
            }
            Console.WriteLine("File successfully uploaded.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private BlobClient CreateBlobClients(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        return blobClient;
    }
}
