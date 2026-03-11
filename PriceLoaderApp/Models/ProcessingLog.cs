namespace PriceLoaderApp.Models;

public class ProcessingLog
{
    public DateTime CreatedAt { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
}
