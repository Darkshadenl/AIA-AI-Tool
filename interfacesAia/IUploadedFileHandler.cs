namespace InterfacesAia;

public interface IUploadedFileHandler
{
    Task Handle(IInputData input, string inputContentType);
    void SetNext(IUploadedFileHandler next);
}
