﻿using BrazaImoveis.Infrastructure.Database;
using BrazaImoveis.Infrastructure.Models;
using BrazaImoveis.WebCrawler.Consts;
using BrazaImoveis.WebCrawler.Extensions;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.Collections.Concurrent;

namespace BrazaImoveis.WebCrawler.WebCrawlerEngine;

public interface IWebCrawlerEngine
{
    Task Run(RealState realState, bool shouldInsertIntoDb = false);
}

public record TempPropertyImage(string PropertyUrl, string ImageUrl);

public class Engine : IWebCrawlerEngine
{
    private readonly IDatabaseClient _databaseClient;
    private readonly ScrapingBrowser _browser;

    private ConcurrentBag<SearchProperty> _propertiesToInsert = new();
    private ConcurrentBag<string> _visitedUrls = new();

    private ParallelOptions ParallelOptions { get; set; } = null!;
    private CancellationTokenSource CancellationTokenSource { get; set; } = null!;

    public Engine(IDatabaseClient databaseClient)
    {
        _databaseClient = databaseClient;
        _browser = new ScrapingBrowser
        {
            IgnoreCookies = true
        };
    }

    public async Task Run(RealState realState, bool shouldInsertIntoDb = false)
    {
        Console.WriteLine($"Starting scrapping {realState.Name}");

        CancellationTokenSource = new CancellationTokenSource();

        ParallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 8,
            CancellationToken = CancellationTokenSource.Token
        };

        var visited = (await _databaseClient.GetAll<SearchProperty>(w => w.RealStateId == realState.Id)).Select(s => s.PropertyUrl.Replace(realState.DomainUrl!, ""));

        _visitedUrls = new ConcurrentBag<string>(visited);

        try
        {
            await Parallel.ForEachAsync(realState.FilterResultsUrls.Split(','), ParallelOptions,
            async (searchUrl, token) =>
            {
                var currentPage = 1;

                if (CancellationTokenSource.IsCancellationRequested) return;

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
                _propertiesToInsert = new ConcurrentBag<SearchProperty>(
                    (await _databaseClient.Insert(_propertiesToInsert)).ToList());
        }

        _visitedUrls.Clear();
        _propertiesToInsert.Clear();
    }

    private async Task FetchPageInformation(RealState realState, string url, int currentPage)
    {
        if (CancellationTokenSource.IsCancellationRequested) return;

        var page = await _browser.NavigateToPageAsync(new Uri(url));

        if (realState.NotFoundXpath != null && page.Html.SelectSingleNode(realState.NotFoundXpath) != null
            && page.Html.SelectSingleNode(realState.NotFoundXpath).InnerText.Contains(realState.NotFoundText))
            return;

        var links = page.GetAllValidLinks(realState).ToList()
                        .Where(s => !_visitedUrls.Contains(s));

        foreach (var link in links)
        {
            var nextUrl = $"{realState.DomainUrl}{link}";

            Console.WriteLine(nextUrl);

            if (_visitedUrls.Contains(nextUrl))
                return;

            if (realState.PaginationType == PaginationTypes.Pagination && nextUrl.Contains(realState.NextPagePrefix))
            {
                var pageNumber = int.Parse(nextUrl.Split('=').Last());

                if (pageNumber <= currentPage || pageNumber - currentPage != 1)
                    return;

                Interlocked.Increment(ref currentPage);
                await FetchPageInformation(realState, nextUrl, currentPage);
            }
            else
                SavePageDetails(realState, await _browser.NavigateToPageAsync(new Uri(nextUrl)));
        }

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
            var price = detailPage.Html.SelectSingleNode(realState.PricingXpath)?.InnerText;
            var title = detailPage.Html.SelectSingleNode(realState.TitleXpath)?.InnerText;
            var images = detailPage.GetAllImagesUrls(realState);
            var description = detailPage.Html.SelectSingleNode(realState.DescriptionXpath)?.InnerText;
            var detailsList = detailPage.Html.SelectSingleNode(realState.DetailsXpath);

            string details = string.Empty;
            if (detailsList != null && detailsList.Name == "div")
            {
                foreach (var child in detailsList.ChildNodes)
                    details += $"{child.InnerText},";

            }
            else if (detailsList != null && detailsList.Name == "ul")
                details = string.Join(", ", detailsList?.CssSelect("li").Select(s => s.InnerText) ?? Array.Empty<string>());

            var bedroomsText = detailPage.Html.SelectSingleNode(realState.FilterBedroomsXpath)?.InnerText;
            var bathroomsText = detailPage.Html.SelectSingleNode(realState.FilterBathroomsXpath)?.InnerText;
            var garageSpaceText = detailPage.Html.SelectSingleNode(realState.FilterGarageSpacesXpath)?.InnerText;
            var priceText = detailPage.Html.SelectSingleNode(realState.FilterCostXpath)?.InnerText;
            var squarefootText = detailPage.Html.SelectSingleNode(realState.FilterSquareFootXpath)?.InnerText;


            var typeText = string.Empty;

            if (string.IsNullOrEmpty(realState.FilterTypeXpath))
            {
                var startUrl = detailPage.AbsoluteUrl.ToString().Replace(realState.DomainUrl, string.Empty);

                foreach (var s in realState.PropertyDetailPrefix.Split(','))
                {
                    if (startUrl.StartsWith(s))
                    {
                        typeText = s.Replace("/", string.Empty).ToUpperInvariant();
                        break;
                    }
                }
            }
            else typeText = detailPage.Html.SelectSingleNode(realState.FilterTypeXpath)?.InnerText;

            _ = decimal.TryParse(string.IsNullOrWhiteSpace(priceText) ? null : priceText.SanitizeToDecimal(), out decimal filterPrice);

            _ = decimal.TryParse(string.IsNullOrWhiteSpace(squarefootText) ? null : squarefootText.SanitizeToDecimal().SanitizeFilters(), out decimal filterSquareFoot);

            var property = new SearchProperty
            {
                RealStateId = realState.Id,
                RealStateName = realState.Name,
                RealStateDomainUrl = realState.DomainUrl,
                PropertyUrl = detailPage.AbsoluteUrl.ToString(),
                PropertyTitle = title?.Sanitize() ?? string.Empty,
                PropertyPrice = price?.Sanitize() ?? string.Empty,
                PropertyDescription = description?.Sanitize() ?? string.Empty,
                PropertyDetails = details?.Sanitize() ?? string.Empty,
                PropertyImages = string.Join(',', images),

                PropertyFilterBedrooms = string.IsNullOrWhiteSpace(bedroomsText) ? null : int.Parse(bedroomsText.SanitizeFilters()),
                PropertyFilterBathrooms = string.IsNullOrWhiteSpace(bathroomsText) ? null : int.Parse(bathroomsText.SanitizeFilters()),
                PropertyFilterGarageSpaces = string.IsNullOrWhiteSpace(garageSpaceText) ? null : int.Parse(garageSpaceText.SanitizeFilters()),
                PropertyFilterPrice = filterPrice == default ? null : filterPrice,
                PropertyFilterSquareFoot = filterSquareFoot == default ? null : filterSquareFoot,
                PropertyFilterType = typeText!,

                // TODO: get from table
                StateId = 23,
                StateKey = "RS",
                StateName = "Rio Grande do Sul",
                CityId = 1,
                CityName = "Capão da Canoa",
            };

            _propertiesToInsert.Add(property);
            _visitedUrls.Add(property.PropertyUrl.Replace(realState.DomainUrl, ""));

            if (_propertiesToInsert.Count >= 50)
                CancellationTokenSource.Cancel(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(detailPage.AbsoluteUrl);
        }
    }
}
