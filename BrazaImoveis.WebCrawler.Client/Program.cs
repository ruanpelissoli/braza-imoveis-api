// See https://aka.ms/new-console-template for more information
using BrazaImoveis.Infrastructure;
using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler;
using BrazaImoveis.WebCrawler.WebCrawlerEngine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serviceProvider = new ServiceCollection()
    .AddInfrastructure(configuration)
    .AddWebCrawlerEngine()
    .BuildServiceProvider();

var engine = serviceProvider.GetRequiredService<IWebCrawlerEnginer>();
var database = serviceProvider.GetRequiredService<IDatabaseClient>();

try
{
    foreach (var model in await database.GetAll<RealState>())
    {
        await engine.Run(model);
        //await engine.FillFilters(model);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.StackTrace);
}

