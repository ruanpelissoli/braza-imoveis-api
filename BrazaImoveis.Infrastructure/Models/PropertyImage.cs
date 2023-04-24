using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("property_image")]
public class PropertyImage : BaseDatabaseModel
{
    [Column("property_id")]
    public long PropertyId { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; } = null!;
}
