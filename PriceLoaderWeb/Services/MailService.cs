using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using PriceLoaderWeb.Configuration;

namespace PriceLoaderWeb.Services;

public class MailService : IMailService
{
    private readonly AppSettings _settings;

    public MailService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<List<(MimePart Part, string FileName)>> GetCsvAttachmentsAsync(string senderEmail)
    {
        var result = new List<(MimePart, string)>();

        using var client = new ImapClient();
        await client.ConnectAsync(
            _settings.Mail.ImapServer,
            _settings.Mail.Port,
            _settings.Mail.UseSsl);

        await client.AuthenticateAsync(
            _settings.Mail.Username,
            _settings.Mail.Password);

        var inbox = client.Inbox;
        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

        var uids = await inbox.SearchAsync(SearchQuery.FromContains(senderEmail));

        foreach (var uid in uids)
        {
            var message = await inbox.GetMessageAsync(uid);
            foreach (var attachment in message.Attachments)
            {
                if (attachment is MimePart part &&
                    part.FileName?.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result.Add((part, part.FileName!));
                }
            }
        }

        await client.DisconnectAsync(true);
        return result;
    }
}
