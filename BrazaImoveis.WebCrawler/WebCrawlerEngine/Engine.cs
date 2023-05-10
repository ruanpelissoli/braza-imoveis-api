using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler.Extensions;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace BrazaImoveis.WebCrawler.WebCrawlerEngine;

public interface IWebCrawlerEnginer
{
    Task Run(RealState realState);
    Task FillFilters(RealState realState);
}

public class Engine : IWebCrawlerEnginer
{
    private readonly IDatabaseClient _databaseClient;
    private readonly ScrapingBrowser _browser;

    private int _currentPage = 1;

    public Engine(IDatabaseClient databaseClient)
    {
        _databaseClient = databaseClient;
        _browser = new ScrapingBrowser();
        _browser.IgnoreCookies = true;
    }

    public async Task Run(RealState realState)
    {
        foreach (var searchUrl in realState.FilterResultsUrls.Split(','))
        {
            await FetchPageInformation(realState, $"{realState.DomainUrl}{searchUrl}");

            _currentPage = 1;
        }
    }

    private async Task FetchPageInformation(RealState realState, string url)
    {
        var page = await _browser.NavigateToPageAsync(new Uri(url));

        var detailsPrefixes = realState.PropertyDetailPrefix.Split(',');

        var links = page.Html
                        .CssSelect("a")
                        .Where(link => link.Attributes.Contains("href"))
                        .ToList()
                        .Where(link =>
                            link.Attributes["href"].Value.StartsWith(detailsPrefixes[0])
                            || link.Attributes["href"].Value.StartsWith($"{realState.DomainUrl}{detailsPrefixes[0]}")
                            || link.Attributes["href"].Value.StartsWith(detailsPrefixes[1])
                            || link.Attributes["href"].Value.StartsWith($"{realState.DomainUrl}{detailsPrefixes[1]}")
                            || (!string.IsNullOrEmpty(realState.NextPagePrefix) && link.Attributes["href"].Value.Contains(realState.NextPagePrefix)))
                        .Distinct();

        foreach (var link in links)
        {
            var nextUrl = link.Attributes["href"].Value.Contains(realState.DomainUrl) ? link.Attributes["href"].Value : $"{realState.DomainUrl}{link.Attributes["href"].Value}";

            // Hit Cache
            if ((await _databaseClient.GetAll<Property>(w => w.Url == nextUrl)).Any())
                continue;

            if (!string.IsNullOrEmpty(realState.NextPagePrefix) && nextUrl.Contains(realState.NextPagePrefix))
            {
                var pageNumber = int.Parse(nextUrl.Split('=').Last());

                if (pageNumber <= _currentPage)
                    continue;

                _currentPage++;
                await FetchPageInformation(realState, nextUrl);
            }
            else
                await SavePageDetails(realState, await _browser.NavigateToPageAsync(new Uri(nextUrl)));
        }
    }

    private async Task SavePageDetails(RealState realState, WebPage detailPage)
    {
        var price = detailPage.Html.SelectSingleNode(realState.PricingXpath).InnerText;

        var title = detailPage.Html.SelectSingleNode(realState.TitleXpath).InnerText;

        var imageLinks = detailPage.Html
                            .CssSelect("a")
                            .Where(link => link.Attributes.Contains("href"))
                            .ToList()
                            .Where(link => link.Attributes["href"].Value.StartsWith(realState.ImagesPrefix)
                             || link.Attributes["href"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        var imageImg = detailPage.Html
                            .CssSelect("img")
                            .Where(link => link.Attributes.Contains("src"))
                            .ToList()
                            .Where(link => link.Attributes["src"].Value.StartsWith(realState.ImagesPrefix)
                             || link.Attributes["src"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        var imageImgDatSrc = detailPage.Html
                            .CssSelect("img")
                            .Where(link => link.Attributes.Contains("data-src"))
                            .ToList()
                            .Where(link => link.Attributes["data-src"].Value.StartsWith(realState.ImagesPrefix)
                             || link.Attributes["data-src"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        var images = imageLinks.Select(s => s.Attributes["href"].Value)
                        .Concat(imageImg.Select(s => s.Attributes["src"].Value))
                        .Concat(imageImgDatSrc.Select(s => s.Attributes["data-src"].Value))
                        .Distinct();

        var description = detailPage.Html.SelectSingleNode(realState.DescriptionXpath)?.InnerText;

        var detailsList = detailPage.Html.SelectSingleNode(realState.DetailsXpath);

        var details = string.Join(", ", detailsList?.CssSelect("li").Select(s => s.InnerText) ?? Array.Empty<string>());

        var bedroomsText = detailPage.Html.SelectSingleNode(realState.FilterBedroomsXpath)?.InnerText;
        var bathroomsText = detailPage.Html.SelectSingleNode(realState.FilterBathroomsXpath)?.InnerText;
        var garageSpaceText = detailPage.Html.SelectSingleNode(realState.FilterGarageSpacesXpath)?.InnerText;
        var priceText = detailPage.Html.SelectSingleNode(realState.FilterCostXpath)?.InnerText;
        var squarefootText = detailPage.Html.SelectSingleNode(realState.FilterSquareFootXpath)?.InnerText;

        var property = new Property
        {
            Url = detailPage.AbsoluteUrl.ToString(),
            RealStateId = realState.Id,
            Title = title.Sanitize(),
            Price = price.Sanitize(),
            Description = description?.Sanitize() ?? string.Empty,
            Details = details?.Sanitize() ?? string.Empty,

            FilterBedrooms = string.IsNullOrWhiteSpace(bedroomsText) ? null : int.Parse(bedroomsText),
            FilterBathrooms = string.IsNullOrWhiteSpace(bathroomsText) ? null : int.Parse(bathroomsText),
            FilterGarageSpaces = string.IsNullOrWhiteSpace(garageSpaceText) ? null : int.Parse(garageSpaceText),
            FilterCost = string.IsNullOrWhiteSpace(priceText) ? null : decimal.Parse(priceText
                .Replace("R$", "")
                .Replace(".", "")
                .Replace(",", ".").Trim()),
            FilterSquareFoot = string.IsNullOrWhiteSpace(squarefootText) ? null : decimal.Parse(squarefootText
                .Replace(".", "")
            .Replace(",", ".").Trim())
        };

        property = await _databaseClient.Insert(property);

        await _databaseClient.Insert(images.Select(image => new PropertyImage
        {
            PropertyId = property.Id,
            ImageUrl = image
        }));
    }

    public async Task FillFilters(RealState realState)
    {
        var properties = await _databaseClient.GetAll<Property>(w => w.RealStateId == realState.Id);

        foreach (var property in properties)
        {
            var page = await _browser.NavigateToPageAsync(new Uri(property.Url));

            var bedroomsText = page.Html.SelectSingleNode(realState.FilterBedroomsXpath).InnerText;
            var bathroomsText = page.Html.SelectSingleNode(realState.FilterBathroomsXpath).InnerText;
            var garageSpaceText = page.Html.SelectSingleNode(realState.FilterGarageSpacesXpath).InnerText;
            var priceText = page.Html.SelectSingleNode(realState.FilterCostXpath).InnerText;
            var squarefootText = page.Html.SelectSingleNode(realState.FilterSquareFootXpath).InnerText;

            property.FilterBedrooms = string.IsNullOrWhiteSpace(bedroomsText) ? null : int.Parse(bedroomsText);
            property.FilterBathrooms = string.IsNullOrWhiteSpace(bathroomsText) ? null : int.Parse(bathroomsText);
            property.FilterGarageSpaces = string.IsNullOrWhiteSpace(garageSpaceText) ? null : int.Parse(garageSpaceText);
            property.FilterCost = string.IsNullOrWhiteSpace(priceText) ? null : decimal.Parse(priceText
                .Replace("R$", "")
                .Replace(".", "")
                .Replace(",", ".").Trim());
            property.FilterSquareFoot = string.IsNullOrWhiteSpace(squarefootText) ? null : decimal.Parse(squarefootText
                .Replace(".", "")
                .Replace(",", ".").Trim());

            await _databaseClient.Update(property);

            Thread.Sleep(2000);
        }
    }
}
