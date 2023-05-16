namespace BrazaImoveis.Contracts.Requests;
public class PropertiesFilterRequest
{
    public string? Type { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? GarageSpaces { get; set; }
    public decimal? Price { get; set; }
    public decimal? SquareFoot { get; set; }
    public long? StateId { get; set; }
    public long? CityId { get; set; }

    public int? Page { get; set; }
    public int? Size { get; set; }
}
