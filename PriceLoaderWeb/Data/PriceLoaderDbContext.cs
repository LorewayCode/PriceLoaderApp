using Microsoft.EntityFrameworkCore;
using PriceLoaderWeb.Models;

namespace PriceLoaderWeb.Data;

public class PriceLoaderDbContext : DbContext
{
    public PriceLoaderDbContext(DbContextOptions<PriceLoaderDbContext> options) 
        : base(options)
    {
    }

    public DbSet<PriceItem> PriceItems { get; set; } = null!;
    public DbSet<ProcessingLog> ProcessingLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.ToTable("priceitems");
            entity.HasIndex(e => e.SearchVendor);
            entity.HasIndex(e => e.SearchNumber);
            entity.HasIndex(e => e.SupplierName);
            entity.HasIndex(e => e.ProcessedAt);
        });

        modelBuilder.Entity<ProcessingLog>(entity =>
        {
            entity.ToTable("processinglogs");
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.SupplierName);
        });
    }
}
