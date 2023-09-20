namespace InterfacesAia;

public interface IUploadedFileHandler
{
    Task Handle(string inputPath, string outputPath, string inputContentType);
    void SetNext(IUploadedFileHandler next);
}
