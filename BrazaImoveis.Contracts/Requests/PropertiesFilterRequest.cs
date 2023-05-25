namespace BrazaImoveis.Contracts.Requests;
public class PropertiesFilterRequest
{
    public string? Type { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? GarageSpaces { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinSquareFoot { get; set; }
    public decimal? MaxSquareFoot { get; set; }
    public long? StateId { get; set; }
    public long? CityId { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 12;
}
