using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler.Extensions;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace BrazaImoveis.WebCrawler.WebCrawlerEngine;

public interface IWebCrawlerEnginer
{
    Task Run(RealState realState);
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
    }

    public async Task SavePageDetails(RealState realState, WebPage detailPage)
    {
        var price = detailPage.Html.SelectSingleNode(realState.PricingXpath).InnerText;

        var title = detailPage.Html.SelectSingleNode(realState.TitleXpath).InnerText;

        var imageLinks = detailPage.Html
                            .CssSelect("a")
                            .Where(link => link.Attributes["href"].Value.StartsWith(realState.ImagesPrefix));

        var imageImg = detailPage.Html
                            .CssSelect("img")
                            .Where(link => link.Attributes["src"].Value.StartsWith(realState.ImagesPrefix));

        var images = imageLinks.Select(s => s.Attributes["href"].Value)
                        .Concat(imageImg.Select(s => s.Attributes["src"].Value))
                        .Distinct();

        var description = detailPage.Html.SelectSingleNode(realState.DescriptionXpath).InnerText;

        var detailsList = detailPage.Html.SelectSingleNode(realState.DetailsXpath);

        var details = string.Join(", ", detailsList.CssSelect("li").Select(s => s.InnerText));

        var property = new Property
        {
            Url = detailPage.AbsoluteUrl.ToString(),
            RealStateId = realState.Id,
            Title = title.Sanitize(),
            Price = price.Sanitize(),
            Description = description.Sanitize(),
            Details = details.Sanitize()
        };

        property = await _databaseClient.Insert(property);

        await _databaseClient.Insert(images.Select(image => new PropertyImage
        {
            PropertyId = property.Id,
            ImageUrl = image
        }));
    }

    public async Task FetchPageInformation(RealState realState, string url)
    {
        var page = await _browser.NavigateToPageAsync(new Uri(url));

        var detailsPrefixes = realState.PropertyDetailPrefix.Split(',');

        var links = page.Html
                        .CssSelect("a")
                        .Where(link =>
                            link.Attributes["href"].Value.StartsWith(detailsPrefixes[0])
                            || link.Attributes["href"].Value.StartsWith(detailsPrefixes[1])
                            || link.Attributes["href"].Value.Contains(realState.NextPagePrefix));

        foreach (var link in links)
        {
            var nextUrl = $"{realState.DomainUrl}{link.Attributes["href"].Value}";

            // Hit Cache
            if ((await _databaseClient.GetAll<Property>(w => w.Url == nextUrl)).Any())
                continue;

            if (nextUrl.Contains(realState.NextPagePrefix))
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

    public async Task Run(RealState realState)
    {
        foreach (var searchUrl in realState.FilterResultsUrls.Split(','))
        {
            await FetchPageInformation(realState, $"{realState.DomainUrl}{searchUrl}");

            _currentPage = 1;
        }
    }
}
