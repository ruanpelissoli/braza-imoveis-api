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

    [Column("filter_bedrooms")]
    public int? FilterBedrooms { get; set; }

    [Column("filter_bathrooms")]
    public int? FilterBathrooms { get; set; }

    [Column("filter_garagespace")]
    public int? FilterGarageSpaces { get; set; }

    [Column("filter_squarefoot")]
    public decimal? FilterSquareFoot { get; set; }

    [Column("filter_cost")]
    public decimal? FilterCost { get; set; }

    [Column("filter_type")]
    public string? FilterType { get; set; } = null!;

    [Column("filter_state")]
    public long? StateId { get; set; }

    [Column("filter_city")]
    public long? CityId { get; set; }
}
