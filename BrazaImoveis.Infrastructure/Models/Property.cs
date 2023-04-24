using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("property")]
public class Property : BaseDatabaseModel
{
    [Column("realstate_id")]
    public long RealStateId { get; set; }

    [Column("url")]
    public string Url { get; set; } = null!;

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("price")]
    public string Price { get; set; } = null!;

    [Column("description")]
    public string Description { get; set; } = null!;

    [Column("details")]
    public string Details { get; set; } = null!;
}
