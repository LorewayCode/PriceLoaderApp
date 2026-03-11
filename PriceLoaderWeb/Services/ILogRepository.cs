using PriceLoaderWeb.Models;

namespace PriceLoaderWeb.Services;

public interface ILogRepository
{
    Task AddLogAsync(ProcessingLog log);
    Task AddLogsAsync(IEnumerable<ProcessingLog> logs);
    Task<List<ProcessingLog>> GetRecentLogsAsync(int count = 50);
    Task ClearLogsAsync();
}
