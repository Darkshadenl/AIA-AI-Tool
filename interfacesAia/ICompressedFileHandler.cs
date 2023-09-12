namespace InterfacesAia;

public interface ICompressedFileHandler
{
    Task<MemoryStream> Handle(MemoryStream input, string extension);
    void SetNext(ICompressedFileHandler next);
}
