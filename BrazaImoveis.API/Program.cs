using BrazaImoveis.API.Middlewares;
using BrazaImoveis.Contracts.Requests;
using BrazaImoveis.Contracts.Responses;
using BrazaImoveis.Infrastructure;
using BrazaImoveis.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});


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

app.UseRateLimiter();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseMiddleware<AuthenticationMiddleware>();

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
    var properties = await client.FilterProperties(new PropertiesFilterRequest
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

app.MapGet("/properties/{id}", async (
    [FromRoute] long id,
    [FromServices] ISupabaseCachedClient cachedClient) =>
{
    var search = await cachedClient.GetPropertyById(id);

    if (search == null)
        return Results.NoContent();

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
        City = search.CityName
    };

    return Results.Ok(response);
})
.WithName("GetProperty")
.WithOpenApi();

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
.WithName("GetCities")
.WithOpenApi();

app.Run();