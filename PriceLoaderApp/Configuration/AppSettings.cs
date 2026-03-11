namespace PriceLoaderApp.Configuration;

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public List<SupplierConfig> Suppliers { get; set; } = new();
}
