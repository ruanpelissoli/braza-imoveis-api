namespace BrazaImoveis.WebCrawler.Extensions;
public static class StringExtensions
{
    public static string Sanitize(this string text)
    {
        return text
                .Replace("\n\r", Environment.NewLine)
                .Trim();
    }
}
