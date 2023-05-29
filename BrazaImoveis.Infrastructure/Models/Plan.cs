using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("plan")]
public class Plan : BaseDatabaseModel
{
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("price")]
    public decimal Price { get; set; }

    [Column("property_quantity")]
    public int PropertyQuantity { get; set; }
}