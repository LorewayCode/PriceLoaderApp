namespace PriceLoaderApp.Configuration;

public class SupplierConfig
{
    public string Name { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Delimiter { get; set; } = ";";
    public Dictionary<string, string> Columns { get; set; } = new();
}
