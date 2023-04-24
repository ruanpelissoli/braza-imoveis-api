using BrazaImoveis.WebCrawler.WebCrawlerEngine;
using Microsoft.Extensions.DependencyInjection;

namespace BrazaImoveis.WebCrawler;
public static class DependencyInjection
{
    public static IServiceCollection AddWebCrawlerEngine(this IServiceCollection services) =>
        services
            .AddScoped<IWebCrawlerEnginer, Engine>();
}
