using BrazaImoveis.API.Filters;
using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler.WebCrawlerEngine;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace BrazaImoveis.API.Modules;

public class PropertyBotModule : CarterModule
{
    public PropertyBotModule() : base("property-bot")
    {

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/run/{realStateId}", async (
            [FromRoute] long realStateId,
            [FromServices] IDatabaseClient _databaseClient,
            [FromServices] IWebCrawlerEngine _webCrawlerEngine) =>
        {
            var realState = (await _databaseClient.GetAll<RealState>(w => w.Id == realStateId)).FirstOrDefault();

            if (realState == null)
                return Results.BadRequest();

            await _webCrawlerEngine.Run(realState, true);

            return Results.Ok();
        })
       .AddEndpointFilter<ApiKeyEndpointFilter>()
       .WithName("PostPropertyBot")
       .WithOpenApi();
    }
}
