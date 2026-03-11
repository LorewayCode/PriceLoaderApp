using System.Text.RegularExpressions;

namespace PriceLoaderWeb.Utils;

public static class CountParser
{
    public static int Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        value = value.Trim();

        if (value.Contains("-"))
            return int.Parse(value.Split('-')[1]);

        if (value.StartsWith(">") || value.StartsWith("<"))
            return int.Parse(Regex.Replace(value, "[^0-9]", ""));

        return int.TryParse(value, out var result) ? result : 0;
    }
}
