using Microsoft.AspNetCore.Mvc;
using PriceLoaderWeb.Models;
using PriceLoaderWeb.Services;
using Microsoft.Extensions.Logging;

namespace PriceLoaderWeb.Controllers;

public class PriceLoaderController : Controller
{
    private readonly IPriceRepository _priceRepo;
    private readonly ILogRepository _logRepo;
    private readonly IPriceLoaderService _loaderService;
    private readonly ILogger<PriceLoaderController> _logger;

    public PriceLoaderController(
        IPriceRepository priceRepo,
        ILogRepository logRepo,
        IPriceLoaderService loaderService,
        ILogger<PriceLoaderController> logger)
    {
        _priceRepo = priceRepo;
        _logRepo = logRepo;
        _loaderService = loaderService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, string? vendor = null, string? number = null)
    {
        const int pageSize = 50;
        int skip = (page - 1) * pageSize;

        List<PriceItem> items;
        int totalCount;

        if (!string.IsNullOrWhiteSpace(vendor) || !string.IsNullOrWhiteSpace(number))
        {
            items = await _priceRepo.SearchAsync(vendor, number, skip, pageSize);
            totalCount = items.Count;
        }
        else
        {
            items = await _priceRepo.GetAllAsync(skip, pageSize);
            totalCount = await _priceRepo.GetTotalCountAsync();
        }

        var logs = await _logRepo.GetRecentLogsAsync(20);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.Vendor = vendor;
        ViewBag.Number = number;

        var model = new PriceLoaderViewModel
        {
            Items = items,
            Logs = logs,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoadPrices()
    {
        _logger.LogInformation("Starting price load...");
        
        try
        {
            var (count, logs) = await _loaderService.RunAsync();
            
            _logger.LogInformation("Loaded {Count} items. Logs count: {LogCount}", count, logs.Count);
            foreach (var log in logs)
            {
                _logger.LogInformation("Log: Supplier={Supplier}, File={File}, Message={Message}, IsError={IsError}", 
                    log.SupplierName, log.FileName, log.Message, log.IsError);
            }
            
            TempData["Message"] = $"Загружено {count} позиций";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading prices");
            TempData["Message"] = $"Ошибка: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ClearPrices()
    {
        await _priceRepo.ClearAllAsync();
        TempData["Message"] = "Все данные очищены";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ClearLogs()
    {
        await _logRepo.ClearLogsAsync();
        TempData["Message"] = "Логи очищены";
        return RedirectToAction(nameof(Index));
    }
}

public class PriceLoaderViewModel
{
    public List<PriceItem> Items { get; set; } = new();
    public List<ProcessingLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
}
