
namespace InterfacesAia.Handlers;

public interface IUploadHandler
{
	void ReceiveFileChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks);
}

