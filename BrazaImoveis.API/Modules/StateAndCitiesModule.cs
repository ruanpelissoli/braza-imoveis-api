using BrazaImoveis.API.Filters;
using BrazaImoveis.Contracts.Responses;
using BrazaImoveis.Infrastructure.Database;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace BrazaImoveis.API.Modules;

public class StateAndCitiesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/states", async (
            [FromServices] ISupabaseCachedClient cachedClient) =>
        {
            var states = await cachedClient.GetStates();

            if (states == null || !states.Any())
                return Results.NoContent();

            var response = states.Select(s =>
            {
                return new GetStatesResponse
                {
                    Id = s.Id,
                    Key = s.Key,
                    Name = s.Name,
                };
            });

            return Results.Ok(response);
        })
       .AddEndpointFilter<ApiKeyEndpointFilter>()
       .WithName("GetStates")
       .WithOpenApi();

        app.MapGet("/cities/{stateId}", async (
            [FromRoute] long stateId,
            [FromServices] ISupabaseCachedClient cachedClient) =>
        {
            var cities = await cachedClient.GetCities(stateId);

            if (cities == null || !cities.Any())
                return Results.NoContent();

            var response = cities.Select(s =>
            {
                return new GetCitiesResponse
                {
                    Id = s.Id,
                    StateId = s.StateId,
                    Name = s.Name,
                };
            });

            return Results.Ok(response);
        })
        .AddEndpointFilter<ApiKeyEndpointFilter>()
        .WithName("GetCities")
        .WithOpenApi();
    }
}
