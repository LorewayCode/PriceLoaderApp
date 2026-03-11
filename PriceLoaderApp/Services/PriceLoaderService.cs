using PriceLoaderApp.Configuration;

namespace PriceLoaderApp.Services;

public class PriceLoaderService
{
    private readonly MailService _mail;
    private readonly CsvProcessor _csv;
    private readonly DatabaseRepository _db;
    private readonly IEnumerable<SupplierConfig> _suppliers;

    public PriceLoaderService(
        MailService mail,
        CsvProcessor csv,
        DatabaseRepository db,
        IEnumerable<SupplierConfig> suppliers)
    {
        _mail = mail;
        _csv = csv;
        _db = db;
        _suppliers = suppliers;
    }

    public async Task RunAsync()
    {
        foreach (var supplier in _suppliers)
        {
            var attachments = await _mail.GetCsvAttachmentsAsync(supplier.SenderEmail);

            foreach (var attachment in attachments)
            {
                using var stream = new MemoryStream();
                attachment.Content.DecodeTo(stream);
                stream.Position = 0;

                var items = _csv.Process(stream, attachment.FileName!, supplier);
                await _db.InsertPriceItemsAsync(items);
            }
        }
    }
}
