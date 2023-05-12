using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler.Consts;
using BrazaImoveis.WebCrawler.Extensions;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Collections.Concurrent;

namespace BrazaImoveis.WebCrawler.WebCrawlerEngine;

public interface IWebCrawlerEnginer
{
    Task Run(RealState realState, bool shouldInsertIntoDb = false);
    Task FixImages(RealState realState);
}

public record TempPropertyImage(string PropertyUrl, string ImageUrl);

public class Engine : IWebCrawlerEnginer
{
    private readonly IDatabaseClient _databaseClient;
    private readonly ScrapingBrowser _browser;

    private ConcurrentBag<Property> _propertiesToInsert = new();
    private ConcurrentBag<TempPropertyImage> _propertyImagesToInsert = new();
    private ConcurrentBag<string> _visitedUrls = new();

    private readonly ParallelOptions _parallelOptions;
    private readonly CancellationTokenSource cancellationTokenSource;

    public Engine(IDatabaseClient databaseClient)
    {
        _databaseClient = databaseClient;
        _browser = new ScrapingBrowser
        {
            IgnoreCookies = true
        };

        cancellationTokenSource = new CancellationTokenSource();

        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = cancellationTokenSource.Token
        };
    }

    // GET ALL PROPERTIES WITHOUT IMAGES
    // LOAD PAGE FOR ALL PROPERTIES URLS
    // GET IMAGES
    // INSERT IMAGES FOR PROPERTY ID
    public async Task FixImages(RealState realState)
    {
        var properties = await _databaseClient.GetAll<Property>(w => w.RealStateId == realState.Id);


        foreach (var property in properties)
        {
            var propertyImages = await _databaseClient.GetAll<PropertyImage>(w => w.PropertyId == property.Id);

            if (propertyImages.Any()) continue;

            var detailPage = await _browser.NavigateToPageAsync(new Uri(property.Url));

            var images = detailPage.GetAllImagesUrls(realState);

            await _databaseClient.Insert(images.Select(image => new PropertyImage
            {
                PropertyId = property.Id,
                ImageUrl = image
            }));
        }
    }

    public async Task Run(RealState realState, bool shouldInsertIntoDb = false)
    {
        var visited = (await _databaseClient.GetAll<Property>(w => w.RealStateId == realState.Id)).Select(s => s.Url.Replace(realState.DomainUrl!, ""));

        _visitedUrls = new ConcurrentBag<string>(visited);

        try
        {
            await Parallel.ForEachAsync(realState.FilterResultsUrls.Split(','), _parallelOptions,
            async (searchUrl, token) =>
            {
                var currentPage = 1;

                if (cancellationTokenSource.IsCancellationRequested) return;

                string url = $"{realState.DomainUrl}{searchUrl}";

                if (realState.PaginationType == PaginationTypes.scroll)
                    url = $"{url}&{realState.NextPagePrefix}1000";

                await FetchPageInformation(realState, url, currentPage);
            });
        }
        catch (Exception) { }


        if (shouldInsertIntoDb)
        {
            if (_propertiesToInsert.Any())
                _propertiesToInsert = new ConcurrentBag<Property>((await _databaseClient.Insert(_propertiesToInsert)).ToList());

            if (_propertiesToInsert.Any() && _propertyImagesToInsert.Any())
                await _databaseClient.Insert(_propertyImagesToInsert.Select(image => new PropertyImage
                {
                    PropertyId = _propertiesToInsert.First(f => f.Url == image.PropertyUrl).Id,
                    ImageUrl = image.ImageUrl
                }));
        }

        _visitedUrls.Clear();
        _propertiesToInsert.Clear();
        _propertyImagesToInsert.Clear();
    }

    private async Task FetchPageInformation(RealState realState, string url, int currentPage)
    {
        if (cancellationTokenSource.IsCancellationRequested) return;

        var page = await _browser.NavigateToPageAsync(new Uri(url));

        if (page.Html.SelectSingleNode(realState.NotFoundXpath) != null
            && page.Html.SelectSingleNode(realState.NotFoundXpath).InnerText.Contains(realState.NotFoundText))
            return;

        var links = page.GetAllValidLinks(realState).ToList()
                        .Where(s => !_visitedUrls.Contains(s));

        await Parallel.ForEachAsync(links, _parallelOptions,
        async (link, ct) =>
        {
            var nextUrl = $"{realState.DomainUrl}{link}";

            Console.WriteLine(nextUrl);

            if (_visitedUrls.Contains(nextUrl))
                return;

            if (realState.PaginationType == PaginationTypes.Pagination && nextUrl.Contains(realState.NextPagePrefix))
            {
                var pageNumber = int.Parse(nextUrl.Split('=').Last());

                if (pageNumber <= currentPage)
                    return;

                Interlocked.Increment(ref currentPage);
                await FetchPageInformation(realState, nextUrl, currentPage);
            }
            else
                SavePageDetails(realState, await _browser.NavigateToPageAsync(new Uri(nextUrl)));

            // Thread.Sleep(TimeSpan.FromSeconds(realState.TimeBetweenRequests));
        });

        if (realState.PaginationType == PaginationTypes.ScrollWithPaging)
        {
            Interlocked.Increment(ref currentPage);

            if (url.Contains(realState.NextPagePrefix))
                url = url.Replace($"{realState.NextPagePrefix}{currentPage - 1}", $"{realState.NextPagePrefix}{currentPage}");
            else
                url = url.Replace("*", $"*/{realState.NextPagePrefix}{currentPage}");

            await FetchPageInformation(realState, url, currentPage);
        }
    }

    private void SavePageDetails(RealState realState, WebPage detailPage)
    {
        try
        {
            var price = detailPage.Html.SelectSingleNode(realState.PricingXpath).InnerText;
            var title = detailPage.Html.SelectSingleNode(realState.TitleXpath).InnerText;
            var images = detailPage.GetAllImagesUrls(realState);
            var description = detailPage.Html.SelectSingleNode(realState.DescriptionXpath)?.InnerText;
            var detailsList = detailPage.Html.SelectSingleNode(realState.DetailsXpath);

            string details = string.Empty;
            if (detailsList != null && detailsList.Name == "div")
            {
                foreach (var child in detailsList.ChildNodes)
                {
                    details += $"{child.InnerText},";
                }
            }
            else if (detailsList != null && detailsList.Name == "ul")
                details = string.Join(", ", detailsList?.CssSelect("li").Select(s => s.InnerText) ?? Array.Empty<string>());

            var bedroomsText = detailPage.Html.SelectSingleNode(realState.FilterBedroomsXpath)?.InnerText;
            var bathroomsText = detailPage.Html.SelectSingleNode(realState.FilterBathroomsXpath)?.InnerText;
            var garageSpaceText = detailPage.Html.SelectSingleNode(realState.FilterGarageSpacesXpath)?.InnerText;
            var priceText = detailPage.Html.SelectSingleNode(realState.FilterCostXpath)?.InnerText;
            var squarefootText = detailPage.Html.SelectSingleNode(realState.FilterSquareFootXpath)?.InnerText;

            _ = decimal.TryParse(string.IsNullOrWhiteSpace(priceText) ? null : priceText
                     .Replace("R$", "")
                     .Replace("&nbsp;", "")
                     .Replace(".", "")
                     .Replace(",", ".").Trim(), out decimal filterCost);

            _ = decimal.TryParse(string.IsNullOrWhiteSpace(squarefootText) ? null : squarefootText
                    .Replace("&nbsp;", "")
                    .Replace(".", "")
                    .Replace(",", ".")
                    .Replace("m²", "")
                    .Trim(), out decimal filterSquareFoot);

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
                FilterCost = filterCost == default ? null : filterCost,
                FilterSquareFoot = filterSquareFoot == default ? null : filterSquareFoot
            };

            _propertiesToInsert.Add(property);
            _visitedUrls.Add(property.Url.Replace(realState.DomainUrl, ""));

            foreach (var img in images)
                _propertyImagesToInsert.Add(new TempPropertyImage(property.Url, img));

            if (_propertiesToInsert.Count >= 50)
                cancellationTokenSource.Cancel(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(detailPage.AbsoluteUrl);
        }
    }
}
