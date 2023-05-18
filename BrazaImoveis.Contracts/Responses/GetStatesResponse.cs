namespace BrazaImoveis.Contracts.Responses;

public class GetStatesResponse
{
    public long Id { get; set; }
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;
}
