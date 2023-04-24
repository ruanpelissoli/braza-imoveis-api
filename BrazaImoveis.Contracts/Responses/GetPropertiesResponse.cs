namespace BrazaImoveis.Contracts.Responses;
public class GetPropertiesResponse
{
    public long Id { get; set; }
    public long RealStateId { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Price { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Details { get; set; } = null!;
    public List<PropertyImageResponse> PropertyImages { get; set; } = new List<PropertyImageResponse>();
}

public class PropertyImageResponse
{
    public string Url { get; set; } = null!;
}
