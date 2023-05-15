namespace BrazaImoveis.Contracts.Responses;
public class GetPropertyResponse
{
    public long Id { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Details { get; set; } = null!;
    public int? FilterBedrooms { get; set; }
    public int? FilterBathrooms { get; set; }
    public int? FilterGarageSpaces { get; set; }
    public decimal? FilterSquareFoot { get; set; }
    public decimal? FilterCost { get; set; }
    public string? FilterType { get; set; } = null!;
    public List<GetPropertyImagesResponse> Images { get; set; } = new List<GetPropertyImagesResponse>();
}
