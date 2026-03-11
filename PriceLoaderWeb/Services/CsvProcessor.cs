using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using PriceLoaderWeb.Configuration;
using PriceLoaderWeb.Models;
using PriceLoaderWeb.Utils;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace PriceLoaderWeb.Services;

public interface ICsvProcessor
{
    IEnumerable<PriceItem> Process(Stream stream, string fileName, SupplierConfig supplier);
}

public class CsvProcessor : ICsvProcessor
{
    private readonly ILogger<CsvProcessor> _logger;

    public CsvProcessor(ILogger<CsvProcessor> logger)
    {
        _logger = logger;
    }

    public IEnumerable<PriceItem> Process(Stream stream, string fileName, SupplierConfig supplier)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();

        if (IsExcelFile(bytes))
        {
            return ProcessExcel(bytes, fileName, supplier);
        }
        
        return ProcessCsv(bytes, fileName, supplier);
    }

    private bool IsExcelFile(byte[] bytes)
    {
        _logger.LogInformation("IsExcelFile check: first 4 bytes = {B0:X2} {B1:X2} {B2:X2} {B3:X2}", 
            bytes.Length > 0 ? bytes[0] : 0, 
            bytes.Length > 1 ? bytes[1] : 0, 
            bytes.Length > 2 ? bytes[2] : 0, 
            bytes.Length > 3 ? bytes[3] : 0);
        
        if (bytes.Length >= 4 && bytes[0] == 0x50 && bytes[1] == 0x4B)
        {
            return true;
        }
        return false;
    }

    private IEnumerable<PriceItem> ProcessCsv(byte[] bytes, string fileName, SupplierConfig supplier)
    {
        _logger.LogInformation("Processing CSV file: {FileName}, Delimiter: {Delimiter}", fileName, supplier.Delimiter);
        _logger.LogInformation("Column mapping: Vendor={Vendor}, Number={Number}, Price={Price}, Count={Count}", 
            supplier.Columns["Vendor"], supplier.Columns["Number"], supplier.Columns["Price"], supplier.Columns["Count"]);

        var encoding = EncodingDetector.Detect(bytes);
        _logger.LogInformation("Detected encoding: {Encoding}", encoding.EncodingName);
        
        using var reader = new StreamReader(new MemoryStream(bytes), encoding);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = supplier.Delimiter,
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var csv = new CsvReader(reader, csvConfig);
        csv.Read();
        csv.ReadHeader();

        var header = csv.HeaderRecord;
        _logger.LogInformation("CSV Headers: {Headers}", string.Join(", ", header ?? Array.Empty<string>()));

        int rowCount = 0;
        while (csv.Read())
        {
            rowCount++;
            var vendor = csv.GetField(supplier.Columns["Vendor"]) ?? string.Empty;
            var number = csv.GetField(supplier.Columns["Number"]) ?? string.Empty;
            var description = csv.GetField(supplier.Columns["Description"]) ?? string.Empty;
            var priceStr = csv.GetField(supplier.Columns["Price"]) ?? string.Empty;
            var countStr = csv.GetField(supplier.Columns["Count"]) ?? string.Empty;

            _logger.LogDebug("Row {Row}: Vendor={Vendor}, Number={Number}, Price={Price}, Count={Count}", 
                rowCount, vendor, number, priceStr, countStr);

            decimal price = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(priceStr))
                {
                    priceStr = priceStr.Replace(",", ".").Trim();
                    price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse price: {Price}", priceStr);
            }

            int count = CountParser.Parse(countStr);

            yield return new PriceItem
            {
                Vendor = vendor,
                Number = number,
                SearchVendor = StringUtils.Normalize(vendor),
                SearchNumber = StringUtils.Normalize(number),
                Description = description,
                Price = price,
                Count = count,
                SupplierName = supplier.Name,
                FileName = fileName,
                ProcessedAt = DateTime.UtcNow
            };
        }

        _logger.LogInformation("Processed {Count} rows from {FileName}", rowCount, fileName);
    }

    private IEnumerable<PriceItem> ProcessExcel(byte[] bytes, string fileName, SupplierConfig supplier)
    {
        _logger.LogInformation("Processing Excel file: {FileName}", fileName);
        _logger.LogInformation("Column mapping: Vendor={Vendor}, Number={Number}, Price={Price}, Count={Count}", 
            supplier.Columns["Vendor"], supplier.Columns["Number"], supplier.Columns["Price"], supplier.Columns["Count"]);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);
        
        var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1);
        if (rows == null)
        {
            _logger.LogWarning("No data rows found in Excel file");
            yield break;
        }

        var headerRow = worksheet.Row(1);
        var headers = new Dictionary<string, int>();
        
        foreach (var cell in headerRow.CellsUsed())
        {
            headers[cell.GetString().Trim()] = cell.Address.ColumnNumber;
        }
        
        _logger.LogInformation("Excel Headers: {Headers}", string.Join(", ", headers.Keys));

        var vendorCol = FindColumn(headers, supplier.Columns["Vendor"]);
        var numberCol = FindColumn(headers, supplier.Columns["Number"]);
        var descCol = FindColumn(headers, supplier.Columns["Description"]);
        var priceCol = FindColumn(headers, supplier.Columns["Price"]);
        var countCol = FindColumn(headers, supplier.Columns["Count"]);

        _logger.LogInformation("Found columns: Vendor={VendorCol}, Number={NumberCol}, Price={PriceCol}, Count={CountCol}", 
            vendorCol, numberCol, priceCol, countCol);

        int rowCount = 0;
        foreach (var row in rows)
        {
            rowCount++;
            var vendor = vendorCol > 0 ? row.Cell(vendorCol).GetString() : string.Empty;
            var number = numberCol > 0 ? row.Cell(numberCol).GetString() : string.Empty;
            var description = descCol > 0 ? row.Cell(descCol).GetString() : string.Empty;
            var priceStr = priceCol > 0 ? row.Cell(priceCol).GetString() : string.Empty;
            var countStr = countCol > 0 ? row.Cell(countCol).GetString() : string.Empty;

            _logger.LogDebug("Row {Row}: Vendor={Vendor}, Number={Number}, Price={Price}, Count={Count}", 
                rowCount, vendor, number, priceStr, countStr);

            decimal price = 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(priceStr))
                {
                    priceStr = priceStr.Replace(",", ".").Trim();
                    price = decimal.Parse(priceStr, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse price: {Price}", priceStr);
            }

            int count = CountParser.Parse(countStr);

            yield return new PriceItem
            {
                Vendor = vendor,
                Number = number,
                SearchVendor = StringUtils.Normalize(vendor),
                SearchNumber = StringUtils.Normalize(number),
                Description = description,
                Price = price,
                Count = count,
                SupplierName = supplier.Name,
                FileName = fileName,
                ProcessedAt = DateTime.UtcNow
            };
        }

        _logger.LogInformation("Processed {Count} rows from {FileName}", rowCount, fileName);
    }

    private int FindColumn(Dictionary<string, int> headers, string columnName)
    {
        foreach (var kvp in headers)
        {
            if (kvp.Key.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return 0;
    }
}
