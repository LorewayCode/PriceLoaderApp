using Microsoft.EntityFrameworkCore;
using PriceLoaderWeb.Data;
using PriceLoaderWeb.Models;

namespace PriceLoaderWeb.Services;

public class PriceRepository : IPriceRepository
{
    private readonly PriceLoaderDbContext _context;

    public PriceRepository(PriceLoaderDbContext context)
    {
        _context = context;
    }

    public async Task AddPriceItemsAsync(IEnumerable<PriceItem> items)
    {
        await _context.PriceItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PriceItem>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _context.PriceItems
            .OrderByDescending(x => x.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.PriceItems.CountAsync();
    }

    public async Task<List<PriceItem>> SearchAsync(string? vendor, string? number, int skip = 0, int take = 100)
    {
        var query = _context.PriceItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(vendor))
        {
            query = query.Where(x => x.SearchVendor.Contains(vendor.ToUpper()));
        }

        if (!string.IsNullOrWhiteSpace(number))
        {
            query = query.Where(x => x.SearchNumber.Contains(number.ToUpper()));
        }

        return await query
            .OrderByDescending(x => x.ProcessedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task ClearAllAsync()
    {
        _context.PriceItems.RemoveRange(_context.PriceItems);
        await _context.SaveChangesAsync();
    }
}
