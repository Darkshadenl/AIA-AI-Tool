namespace aia_api.Application.Middleware;

public class EmptyFileCheckMiddleware
{
    private readonly RequestDelegate _next;

    public EmptyFileCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.ContentLength == 0)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("No file received or file is empty.");
            return;
        }

        var form = await context.Request.ReadFormAsync();
        var file = form.Files.Count > 0 ? form.Files[0] : null;

        if (file == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("No file received or file is empty.");
            return;
        }

        await next(context);
    }
}

public static class EmptyFileCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseEmptyFileCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EmptyFileCheckMiddleware>();
    }
}
