namespace PriceLoaderWeb.Configuration;

public class AppSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public MailSettings Mail { get; set; } = new();
    public List<SupplierConfig> Suppliers { get; set; } = new();
}

public class MailSettings
{
    public string ImapServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SupplierConfig
{
    public string Name { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Delimiter { get; set; } = ";";
    public Dictionary<string, string> Columns { get; set; } = new();
}
