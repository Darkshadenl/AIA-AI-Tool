using System;
namespace InterfacesAia
{
	public interface IUploadController
	{
		public void ReceiveFileChunk(string fileName, string contentType, byte[] chunk, int index, int totalChunks);
		public void ZipHandler(string fileName, string contentType);
    }
}

