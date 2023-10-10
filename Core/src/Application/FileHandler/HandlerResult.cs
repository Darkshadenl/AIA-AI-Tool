using System.Net;
using InterfacesAia;

namespace aia_api.Application.FileHandler;

public class HandlerResult : IHandlerResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
