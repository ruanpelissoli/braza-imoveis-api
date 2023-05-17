using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;
[Table("search_property")]
public class SearchProperty : BaseDatabaseModel
{
    [Column("realstate_id")]
    public long RealStateId { get; set; }

    [Column("realstate_name")]
    public string RealStateName { get; set; } = null!;

    [Column("realstate_domainurl")]
    public string RealStateDomainUrl { get; set; } = null!;

    [Column("property_url")]
    public string PropertyUrl { get; set; } = null!;

    [Column("property_title")]
    public string PropertyTitle { get; set; } = null!;

    [Column("property_price")]
    public string PropertyPrice { get; set; } = null!;

    [Column("property_description")]
    public string PropertyDescription { get; set; } = null!;

    [Column("property_details")]
    public string PropertyDetails { get; set; } = null!;

    [Column("property_filter_bedrooms")]
    public int? PropertyFilterBedrooms { get; set; }

    [Column("property_filter_bathrooms")]
    public int? PropertyFilterBathrooms { get; set; }

    [Column("property_filter_garagespaces")]
    public int? PropertyFilterGarageSpaces { get; set; }

    [Column("property_filter_squarefoot")]
    public decimal? PropertyFilterSquareFoot { get; set; }

    [Column("property_filter_price")]
    public decimal? PropertyFilterPrice { get; set; }

    [Column("property_filter_type")]
    public string PropertyFilterType { get; set; } = null!;

    [Column("property_images")]
    public string PropertyImages { get; set; } = null!;

    [Column("state_id")]
    public long StateId { get; set; }

    [Column("state_key")]
    public string StateKey { get; set; } = null!;

    [Column("state_name")]
    public string StateName { get; set; } = null!;

    [Column("city_id")]
    public long CityId { get; set; }

    [Column("city_name")]
    public string CityName { get; set; } = null!;
}
