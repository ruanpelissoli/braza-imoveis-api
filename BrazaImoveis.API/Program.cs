using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Contracts.Responses;
using BrazaImoveis.Infrastructure;
using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    })
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/properties", async (
    [FromQuery] string? type,
    [FromQuery] int? bedrooms,
    [FromQuery] int? bathrooms,
    [FromQuery] int? garageSpaces,
    [FromQuery] decimal? price,
    [FromQuery] decimal? squareFoot,
    [FromQuery] long? stateId,
    [FromQuery] long? cityId,
    [FromQuery] int? page,
    [FromQuery] int? size,
    [FromServices] IDatabaseClient client) =>
{

    var filters = new PropertiesFilterRequest
    {
        Type = type,
        Bedrooms = bedrooms,
        Bathrooms = bathrooms,
        GarageSpaces = garageSpaces,
        Price = price,
        SquareFoot = squareFoot,
        StateId = stateId,
        CityId = cityId,
        Page = page ?? 0,
        Size = size ?? 10
    };

    var properties = await client.FilterProperties(filters);

    var responseList = new List<GetPropertiesResponse>();

    foreach (var property in properties)
    {
        var propertyResponse = new GetPropertiesResponse
        {
            Id = property.Id,
            RealStateId = property.RealStateId,
            Url = property.Url,
            Title = property.Title,
            Price = property.Price,
            FilterBathrooms = property.FilterBathrooms,
            FilterBedrooms = property.FilterBedrooms,
            FilterCost = property.FilterCost,
            FilterGarageSpaces = property.FilterGarageSpaces,
            FilterSquareFoot = property.FilterSquareFoot,
            FilterType = property.FilterType,
        };

        var images = await client.GetAll<PropertyImage>(p => p.PropertyId == property.Id);

        propertyResponse.PropertyImages.AddRange(
            images
            .Take(3)
            .Select(s => new GetPropertyImagesResponse
            {
                Url = s.ImageUrl
            }));

        responseList.Add(propertyResponse);
    }

    return Results.Ok(responseList);
})
.WithName("GetProperties")
.WithOpenApi();

app.MapGet("/properties/{id}", async (
    [FromRoute] long id,
    [FromServices] IDatabaseClient client) =>
{
    var property = await client.GetById<Property>(id);

    if (property == null)
        return Results.NoContent();

    var response = new GetPropertyResponse
    {
        Id = property.Id,
        Url = property.Url,
        Title = property.Title,
        Price = property.Price,
        Description = property.Description,
        Details = property.Details,
        FilterBathrooms = property.FilterBathrooms,
        FilterBedrooms = property.FilterBedrooms,
        FilterCost = property.FilterCost,
        FilterGarageSpaces = property.FilterGarageSpaces,
        FilterSquareFoot = property.FilterSquareFoot,
        FilterType = property.FilterType,
    };

    var images = await client.GetAll<PropertyImage>(p => p.PropertyId == property.Id);

    if (images != null && images.Any())
        response.Images.AddRange(images.Select(s => new GetPropertyImagesResponse
        {
            Url = s.ImageUrl
        }));

    return Results.Ok(response);
})
.WithName("GetProperty")
.WithOpenApi();

app.Run();