using PriceLoaderWeb.Models;

namespace PriceLoaderWeb.Services;

public interface IPriceRepository
{
    Task AddPriceItemsAsync(IEnumerable<PriceItem> items);
    Task<List<PriceItem>> GetAllAsync(int skip = 0, int take = 100);
    Task<int> GetTotalCountAsync();
    Task<List<PriceItem>> SearchAsync(string? vendor, string? number, int skip = 0, int take = 100);
    Task ClearAllAsync();
}
