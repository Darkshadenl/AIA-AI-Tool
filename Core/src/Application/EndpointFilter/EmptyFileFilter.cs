namespace aia_api.Application.EndpointFilter;

public class EmptyFileFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            if (context.HttpContext.Request.ContentLength == 0)
            {
                context.HttpContext.Response.StatusCode = 400;
                await context.HttpContext.Response.WriteAsync("No file received or file is empty.");
                return null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var form = context.HttpContext.Request.ReadFormAsync();
        var file = form.Result.Files.Count > 0 ? form.Result.Files[0] : null;

        if (file == null)
        {
            context.HttpContext.Response.StatusCode = 400;
            await context.HttpContext.Response.WriteAsync("No file received or file is empty.");
            return null;
        }

        return await next(context);
    }
}
