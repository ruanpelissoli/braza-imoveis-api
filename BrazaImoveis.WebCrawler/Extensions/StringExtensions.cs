namespace BrazaImoveis.WebCrawler.Extensions;
public static class StringExtensions
{
    public static string Sanitize(this string text)
    {
        return text
            .Replace("\n\r", Environment.NewLine)
            .Replace("\r\n", Environment.NewLine)
            .Replace("\t", string.Empty)
            .Replace("&nbsp;", " ")
            .Trim();
    }

    public static string SanitizeToDecimal(this string text)
    {
        return text
            .Replace("R$", "")
            .Replace("&nbsp;", "")
            .Replace(".", "")
            .Replace(",", ".")
            .Replace("m²", "")
            .Trim();
    }

}
