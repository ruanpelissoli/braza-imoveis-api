using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("city")]
public class City : BaseDatabaseModel
{
    [Column("state_id")]
    public long StateId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
}
