namespace InterfacesAia;

public interface IUploadedFileHandler
{
    Task<MemoryStream> Handle(IInputData input, string inputContentType);
    void SetNext(IUploadedFileHandler next);
}
