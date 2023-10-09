namespace InterfacesAia;

public interface IFileHandlerFactory
{
    IUploadedFileHandler GetFileHandler();
}
