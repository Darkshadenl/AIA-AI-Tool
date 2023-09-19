namespace InterfacesAia;

public interface IUploadedFileHandler
{
    Task Handle(string path, string inputContentType);
    void SetNext(IUploadedFileHandler next);
}
