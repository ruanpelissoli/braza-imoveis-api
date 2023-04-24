using Postgrest.Attributes;
using Postgrest.Models;

namespace BrazaImoveis.Infrastructure.Models;
public class BaseDatabaseModel : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
