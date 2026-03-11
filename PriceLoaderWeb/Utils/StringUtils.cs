using System.Text.RegularExpressions;

namespace PriceLoaderWeb.Utils;

public static class StringUtils
{
    public static string Normalize(string input)
    {
        return Regex.Replace(input ?? string.Empty, "[^A-Za-z0-9]", "")
            .ToUpperInvariant();
    }
}
