using InterfacesAia;

namespace aia_api.Application.FileHandler.InputTypes;

public class MemoryStreamFileData : IInputData
{
    public MemoryStream Stream { get; set; }
}
