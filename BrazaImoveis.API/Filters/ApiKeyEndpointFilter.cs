namespace BrazaImoveis.API.Filters;

public class ApiKeyEndpointFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;

    public ApiKeyEndpointFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        //if (!context.HttpContext.Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey))
        //    return TypedResults.Unauthorized();

        //var apiKey = _configuration.GetValue<string>("ApiKey");

        //if (!string.IsNullOrEmpty(extractedApiKey) && extractedApiKey.Equals(apiKey))
        //    return TypedResults.Unauthorized();

        return await next(context);
    }
}
