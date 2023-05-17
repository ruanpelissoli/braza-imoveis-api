namespace BrazaImoveis.API.Middlewares;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        //if (!context.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        //{
        //    context.Response.StatusCode = 401;
        //    await context.Response.WriteAsync("Api Key was not provided. (Using ApiKeyMiddleware) ");
        //    return;
        //}

        //var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();

        //var apiKey = appSettings.GetValue<string>("ApiKey");

        //if (!string.IsNullOrEmpty(extractedApiKey) && extractedApiKey.Equals(apiKey))
        //{
        //    await _next(context);
        //}
        //else
        //{
        //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //    await context.Response.WriteAsync("Unauthorized");
        //}
    }
}
