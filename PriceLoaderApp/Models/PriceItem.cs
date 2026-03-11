namespace PriceLoaderApp.Models;

public class PriceItem
{
    public string Vendor { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string SearchVendor { get; set; } = string.Empty;
    public string SearchNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }

    public string SupplierName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
