using MimeKit;

namespace PriceLoaderWeb.Services;

public interface IMailService
{
    Task<List<(MimePart Part, string FileName)>> GetCsvAttachmentsAsync(string senderEmail);
}
