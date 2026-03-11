using Microsoft.Extensions.Configuration;
using Npgsql;
using PriceLoaderApp.Models;


namespace PriceLoaderApp.Services;

public class DatabaseRepository
{
    private readonly string _connectionString;

    public DatabaseRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Postgres")!;
    }

    public async Task InsertPriceItemsAsync(IEnumerable<PriceItem> items)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        foreach (var item in items)
        {
            await using var cmd = new NpgsqlCommand(@"
INSERT INTO PriceItems
(Vendor, Number, SearchVendor, SearchNumber, Description, Price, Count, SupplierName, FileName, ProcessedAt)
VALUES (@v,@n,@sv,@sn,@d,@p,@c,@s,@f,@t)", conn);

            cmd.Parameters.AddWithValue("v", item.Vendor);
            cmd.Parameters.AddWithValue("n", item.Number);
            cmd.Parameters.AddWithValue("sv", item.SearchVendor);
            cmd.Parameters.AddWithValue("sn", item.SearchNumber);
            cmd.Parameters.AddWithValue("d", item.Description);
            cmd.Parameters.AddWithValue("p", item.Price);
            cmd.Parameters.AddWithValue("c", item.Count);
            cmd.Parameters.AddWithValue("s", item.SupplierName);
            cmd.Parameters.AddWithValue("f", item.FileName);
            cmd.Parameters.AddWithValue("t", item.ProcessedAt);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
