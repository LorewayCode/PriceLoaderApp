using CsvHelper;
using CsvHelper.Configuration;
using PriceLoaderApp.Configuration;
using PriceLoaderApp.Models;
using PriceLoaderApp.Utils;
using System.Globalization;
using System.Text;

namespace PriceLoaderApp.Services;

public class CsvProcessor
{
    public IEnumerable<PriceItem> Process(
        Stream stream,
        string fileName,
        SupplierConfig supplier)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();

        var encoding = EncodingDetector.Detect(bytes);
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

        while (csv.Read())
        {
            var vendor = csv.GetField(supplier.Columns["Vendor"]) ?? string.Empty;
            var number = csv.GetField(supplier.Columns["Number"]) ?? string.Empty;


            yield return new PriceItem
            {
                Vendor = vendor,
                Number = number,
                SearchVendor = StringUtils.Normalize(vendor),
                SearchNumber = StringUtils.Normalize(number),
                Description = (csv.GetField(supplier.Columns["Description"]) ?? string.Empty)
                    .Truncate(512),

                Price = csv.GetField<decimal>(supplier.Columns["Price"]),
                Count = CountParser.Parse(csv.GetField(supplier.Columns["Count"]) ?? string.Empty),
                SupplierName = supplier.Name,
                FileName = fileName,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}
