namespace aia_api.Application.Helpers;

public class EnvHelper
{
    public static bool IsDev()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}
