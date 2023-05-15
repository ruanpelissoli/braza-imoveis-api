﻿namespace BrazaImoveis.Contracts.Responses;
public class GetPropertiesResponse
{
    public long Id { get; set; }
    public long RealStateId { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Price { get; set; } = null!;
    public int? FilterBedrooms { get; set; }
    public int? FilterBathrooms { get; set; }
    public int? FilterGarageSpaces { get; set; }
    public decimal? FilterSquareFoot { get; set; }
    public decimal? FilterCost { get; set; }
    public string? FilterType { get; set; } = null!;
    public List<GetPropertyImagesResponse> PropertyImages { get; set; } = new List<GetPropertyImagesResponse>();
}
