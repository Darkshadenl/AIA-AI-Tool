using System;
namespace InterfacesAia
{
	public interface IUploadController
	{
		public void ZipHandler(string fileName, string contentType, string file);
    }
}

