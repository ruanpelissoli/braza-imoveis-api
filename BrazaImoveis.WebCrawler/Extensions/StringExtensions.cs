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

    public static string SanitizeFilters(this string text)
    {
        return text
           .Replace("quarto", "")
           .Replace("banheiro", "")
           .Replace("vaga", "")
           .Replace("(s)", "")
           .Replace("área", "")
           .Replace("area", "")
           .Replace("útil", "")
           .Replace("util", "")
           .Trim();
    }
}
