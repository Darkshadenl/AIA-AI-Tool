namespace InterfacesAia;

public interface IUploadedFileHandler
{
    Task<MemoryStream> Handle(MemoryStream input, string extension);
    void SetNext(IUploadedFileHandler next);
}
