using Azure.Storage;
using Azure.Storage.Blobs;
using DotNetEnv;

namespace aia_api.Application.Azure;

public class AzureClient
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _blobContainerName;

    public AzureClient()
    {
        var blobServiceEndpoint = Env.GetString("BLOBSERVICEENDPOINT");
        var storageAccountKey = Env.GetString("STORAGEACCOUNTKEY");
        var accountName = Env.GetString("ACCOUNTNAME");
        _blobContainerName = Env.GetString("BLOBCONTAINERNAME");

        _blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint),
            new StorageSharedKeyCredential(accountName, storageAccountKey));
    }

    public async Task Pipeline(MemoryStream stream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);

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

}
