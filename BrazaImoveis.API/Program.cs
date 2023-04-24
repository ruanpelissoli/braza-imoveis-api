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
    [FromServices] IDatabaseClient client) =>
{
    var realStates = await client.GetAll<RealState>();

    var responseList = new List<GetPropertiesResponse>();

    var properties = await client.GetAll<Property>();

    foreach (var property in properties)
    {
        var propertyResponse = new GetPropertiesResponse
        {
            Id = property.Id,
            RealStateId = property.RealStateId,
            Url = property.Url,
            Title = property.Title,
            Price = property.Price,
            Description = property.Description,
            Details = property.Details,
        };

        var images = await client.GetAll<PropertyImage>(p => p.PropertyId == property.Id);

        propertyResponse.PropertyImages.AddRange(images.Select(s => new PropertyImageResponse
        {
            Url = s.ImageUrl
        }));

        responseList.Add(propertyResponse);
    }

    return Results.Ok(responseList);
})
.WithName("GetProperties")
.WithOpenApi();

app.Run();