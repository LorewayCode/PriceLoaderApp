using Microsoft.Extensions.Configuration;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;


namespace PriceLoaderApp.Services;

public class MailService
{
    private readonly IConfiguration _config;

    public MailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<List<MimePart>> GetCsvAttachmentsAsync(string senderEmail)
    {
        var result = new List<MimePart>();

        using var client = new ImapClient();
        await client.ConnectAsync(
            _config["Mail:ImapServer"],
            int.Parse(_config["Mail:Port"]!),
            true);

        await client.AuthenticateAsync(
            _config["Mail:Username"],
            _config["Mail:Password"]);

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
                    result.Add(part);
                }
            }
        }

        await client.DisconnectAsync(true);
        return result;
    }
}
