using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("state")]
public class State : BaseDatabaseModel
{
    [Column("key")]
    public string Key { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;
}
