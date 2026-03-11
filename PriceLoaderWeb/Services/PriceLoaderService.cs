using PriceLoaderWeb.Configuration;
using PriceLoaderWeb.Models;
using Microsoft.Extensions.Logging;

namespace PriceLoaderWeb.Services;

public interface IPriceLoaderService
{
    Task<(int ItemsLoaded, List<ProcessingLog> Logs)> RunAsync();
    event EventHandler<string>? StatusChanged;
}

public class PriceLoaderService : IPriceLoaderService
{
    private readonly IMailService _mail;
    private readonly ICsvProcessor _csv;
    private readonly IPriceRepository _priceRepo;
    private readonly ILogRepository _logRepo;
    private readonly IEnumerable<SupplierConfig> _suppliers;
    private readonly ILogger<PriceLoaderService> _logger;

    public event EventHandler<string>? StatusChanged;

    public PriceLoaderService(
        IMailService mail,
        ICsvProcessor csv,
        IPriceRepository priceRepo,
        ILogRepository logRepo,
        IEnumerable<SupplierConfig> suppliers,
        ILogger<PriceLoaderService> logger)
    {
        _mail = mail;
        _csv = csv;
        _priceRepo = priceRepo;
        _logRepo = logRepo;
        _suppliers = suppliers;
        _logger = logger;
    }

    public async Task<(int ItemsLoaded, List<ProcessingLog> Logs)> RunAsync()
    {
        var logs = new List<ProcessingLog>();
        int totalItems = 0;

        _logger.LogInformation("Starting price loading for {Count} suppliers", _suppliers.Count());

        foreach (var supplier in _suppliers)
        {
            _logger.LogInformation("Processing supplier: {Name}, SenderEmail: {Email}", supplier.Name, supplier.SenderEmail);
            StatusChanged?.Invoke(this, $"Обработка поставщика: {supplier.Name}");
            
            try
            {
                _logger.LogInformation("Fetching attachments from {Email}", supplier.SenderEmail);
                var attachments = await _mail.GetCsvAttachmentsAsync(supplier.SenderEmail);
                _logger.LogInformation("Found {Count} attachments", attachments.Count());

                foreach (var attachment in attachments)
                {
                    _logger.LogInformation("Processing attachment: {FileName}", attachment.FileName);
                    
                    using var stream = new MemoryStream();
                    attachment.Part.Content.DecodeTo(stream);
                    stream.Position = 0;

                    var items = _csv.Process(stream, attachment.FileName, supplier).ToList();
                    _logger.LogInformation("Parsed {Count} items from {FileName}", items.Count, attachment.FileName);
                    
                    if (items.Count > 0)
                    {
                        await _priceRepo.AddPriceItemsAsync(items);
                        totalItems += items.Count;
                        
                        logs.Add(new ProcessingLog
                        {
                            SupplierName = supplier.Name,
                            FileName = attachment.FileName,
                            Message = $"Загружено {items.Count} позиций",
                            IsError = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing supplier {Name}", supplier.Name);
                logs.Add(new ProcessingLog
                {
                    SupplierName = supplier.Name,
                    Message = $"Ошибка: {ex.Message}",
                    IsError = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        if (logs.Count > 0)
        {
            await _logRepo.AddLogsAsync(logs);
        }

        StatusChanged?.Invoke(this, $"Загрузка завершена. Всего загружено: {totalItems}");
        return (totalItems, logs);
    }
}
