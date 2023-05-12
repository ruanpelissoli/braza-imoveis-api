using Postgrest.Attributes;

namespace BrazaImoveis.Infrastructure.Models;

[Table("real_state")]
public class RealState : BaseDatabaseModel
{
    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("filter_results_urls")]
    public string FilterResultsUrls { get; set; } = null!;

    [Column("domain_url")]
    public string DomainUrl { get; set; } = null!;

    [Column("next_page_prefix")]
    public string NextPagePrefix { get; set; } = null!;

    [Column("property_detail_prefix")]
    public string PropertyDetailPrefix { get; set; } = null!;
    [Column("pricing_xpath")]
    public string PricingXpath { get; set; } = null!;

    [Column("title_xpath")]
    public string TitleXpath { get; set; } = null!;

    [Column("images_prefix")]
    public string ImagesPrefix { get; set; } = null!;

    [Column("description_xpath")]
    public string DescriptionXpath { get; set; } = null!;

    [Column("details_xpath")]
    public string DetailsXpath { get; set; } = null!;

    [Column("filter_bedrooms_xpath")]
    public string FilterBedroomsXpath { get; set; } = null!;

    [Column("filter_bathrooms_xpath")]
    public string FilterBathroomsXpath { get; set; } = null!;

    [Column("filter_garagespace_xpath")]
    public string FilterGarageSpacesXpath { get; set; } = null!;

    [Column("filter_squarefoot_xpath")]
    public string FilterSquareFootXpath { get; set; } = null!;

    [Column("filter_cost_xpath")]
    public string FilterCostXpath { get; set; } = null!;

    [Column("pagination_type")]
    public string PaginationType { get; set; } = null!;

    [Column("notfound_xpath")]
    public string NotFoundXpath { get; set; } = null!;

    [Column("notfound_text")]
    public string NotFoundText { get; set; } = null!;

    [Column("time_between_requests")]
    public int TimeBetweenRequests { get; set; }
}
