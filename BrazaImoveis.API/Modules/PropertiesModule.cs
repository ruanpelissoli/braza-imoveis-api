﻿using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Contracts.Responses;
using BrazaImoveis.Infrastructure.Database;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace BrazaImoveis.API.Modules;

public class PropertiesModule : CarterModule
{
    public PropertiesModule() : base("properties")
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            [FromQuery] string? type,
            [FromQuery] int? bedrooms,
            [FromQuery] int? bathrooms,
            [FromQuery] int? garageSpaces,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] decimal? minSquareFoot,
            [FromQuery] decimal? maxSquareFoot,
            [FromQuery] long? stateId,
            [FromQuery] long? cityId,
            [FromQuery] int? page,
            [FromQuery] int? size,
            [FromServices] IDatabaseClient _databaseClient) =>
        {
            var properties = await _databaseClient.FilterProperties(new PropertiesFilterRequest
            {
                Type = type,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                GarageSpaces = garageSpaces,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinSquareFoot = minSquareFoot,
                MaxSquareFoot = maxSquareFoot,
                StateId = stateId,
                CityId = cityId,
                Page = page ?? 1,
                Size = size ?? 12
            });

            var responseList = properties.Select(search =>
            {
                return new GetPropertiesResponse()
                {
                    Id = search.Id,
                    RealStateId = search.RealStateId,
                    RealStateName = search.RealStateName,
                    Url = search.PropertyUrl,
                    Title = search.PropertyTitle,
                    Price = search.PropertyPrice,
                    FilterBathrooms = search.PropertyFilterBathrooms,
                    FilterBedrooms = search.PropertyFilterBedrooms,
                    FilterCost = search.PropertyFilterPrice,
                    FilterGarageSpaces = search.PropertyFilterGarageSpaces,
                    FilterSquareFoot = search.PropertyFilterSquareFoot,
                    FilterType = search.PropertyFilterType,
                    Images = search.PropertyImages.Split(",").Take(3).ToArray(),
                    State = search.StateName,
                    City = search.CityName
                };
            });

            return Results.Ok(responseList);
        })
        .WithName("GetProperties")
        .WithOpenApi();

        app.MapGet("/{id}", async (
            [FromRoute] long id,
            [FromServices] ISupabaseCachedClient _cachedDatabaseClient) =>
        {
            var search = await _cachedDatabaseClient.GetPropertyById(id);

            if (search == null)
                return Results.NoContent();

            var similarProperties = await _cachedDatabaseClient.GetSimilarProperties(search);

            var response = new GetPropertyResponse
            {
                Id = search.Id,
                RealStateId = search.RealStateId,
                RealStateName = search.RealStateName,
                Url = search.PropertyUrl,
                Title = search.PropertyTitle,
                Price = search.PropertyPrice,
                Description = search.PropertyDescription,
                Details = search.PropertyDetails,
                FilterBathrooms = search.PropertyFilterBathrooms,
                FilterBedrooms = search.PropertyFilterBedrooms,
                FilterCost = search.PropertyFilterPrice,
                FilterGarageSpaces = search.PropertyFilterGarageSpaces,
                FilterSquareFoot = search.PropertyFilterSquareFoot,
                FilterType = search.PropertyFilterType,
                Images = search.PropertyImages.Split(",").ToArray(),
                State = search.StateName,
                City = search.CityName,
                SimilarProperties = similarProperties.Select(search =>
                {
                    return new GetPropertiesResponse()
                    {
                        Id = search.Id,
                        RealStateId = search.RealStateId,
                        RealStateName = search.RealStateName,
                        Url = search.PropertyUrl,
                        Title = search.PropertyTitle,
                        Price = search.PropertyPrice,
                        FilterBathrooms = search.PropertyFilterBathrooms,
                        FilterBedrooms = search.PropertyFilterBedrooms,
                        FilterCost = search.PropertyFilterPrice,
                        FilterGarageSpaces = search.PropertyFilterGarageSpaces,
                        FilterSquareFoot = search.PropertyFilterSquareFoot,
                        FilterType = search.PropertyFilterType,
                        Images = search.PropertyImages.Split(",").Take(3).ToArray(),
                        State = search.StateName,
                        City = search.CityName
                    };
                })
            };

            return Results.Ok(response);
        })
        .WithName("GetProperty")
        .WithOpenApi();
    }
}
