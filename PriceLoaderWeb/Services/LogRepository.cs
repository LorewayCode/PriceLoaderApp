using Microsoft.EntityFrameworkCore;
using PriceLoaderWeb.Data;
using PriceLoaderWeb.Models;

namespace PriceLoaderWeb.Services;

public class LogRepository : ILogRepository
{
    private readonly PriceLoaderDbContext _context;

    public LogRepository(PriceLoaderDbContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(ProcessingLog log)
    {
        _context.ProcessingLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task AddLogsAsync(IEnumerable<ProcessingLog> logs)
    {
        await _context.ProcessingLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProcessingLog>> GetRecentLogsAsync(int count = 50)
    {
        return await _context.ProcessingLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task ClearLogsAsync()
    {
        _context.ProcessingLogs.RemoveRange(_context.ProcessingLogs);
        await _context.SaveChangesAsync();
    }
}
