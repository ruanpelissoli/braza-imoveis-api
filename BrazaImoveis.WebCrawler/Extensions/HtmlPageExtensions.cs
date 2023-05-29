using BrazaImoveis.Infrastructure.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace BrazaImoveis.WebCrawler.Extensions;
public static class HtmlPageExtensions
{
    public static IEnumerable<string> GetAllImagesUrls(this WebPage webPage, RealState realState)
    {
        var imageLinks = webPage.Html
                            .CssSelect("a")
                            .Where(link => link.Attributes.Contains("href"))
                            .ToList()
                            .Where(link => link.Attributes["href"].Value.StartsWith(realState.ImagesPrefix)
                                || link.Attributes["href"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        var imageImg = webPage.Html
                            .CssSelect("img")
                            .Where(link => link.Attributes.Contains("src"))
                            .ToList()
                            .Where(link => link.Attributes["src"].Value.StartsWith(realState.ImagesPrefix)
                                || link.Attributes["src"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        var imageImgDatSrc = webPage.Html
                            .CssSelect("img")
                            .Where(link => link.Attributes.Contains("data-src"))
                            .ToList()
                            .Where(link => link.Attributes["data-src"].Value.StartsWith(realState.ImagesPrefix)
                                || link.Attributes["data-src"].Value.StartsWith($"{realState.DomainUrl}{realState.ImagesPrefix}"));

        return imageLinks.Select(s => s.Attributes["href"].Value)
                        .Concat(imageImg.Select(s => s.Attributes["src"].Value))
                        .Concat(imageImgDatSrc.Select(s => s.Attributes["data-src"].Value))
                        .Distinct();

    }

    public static IEnumerable<string> GetAllValidLinks(this WebPage webPage, RealState realState)
    {
        var detailsPrefixes = realState.PropertyDetailPrefix.Split(',');

        var linkQuery = webPage.Html
                        .CssSelect("a")
                        .Where(link => link.Attributes.Contains("href"))
                        .Select(link => link.Attributes["href"].Value.Replace(realState.DomainUrl, ""));

        var filteredLinks = new List<string>();
        foreach (var prefix in detailsPrefixes)
        {
            filteredLinks.AddRange(linkQuery.Where(link => link.StartsWith(prefix)));
        }
        linkQuery = linkQuery.Where(filteredLinks.Contains);

        return linkQuery.Distinct().OrderByDescending(s => s);
    }
}
