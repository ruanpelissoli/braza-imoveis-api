namespace BrazaImoveis.Contracts.Responses;

public class GetCitiesResponse
{
    public long Id { get; set; }
    public long StateId { get; set; }
    public string Name { get; set; } = null!;
}
