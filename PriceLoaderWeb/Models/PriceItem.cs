using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceLoaderWeb.Models;

public class PriceItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [MaxLength(64)]
    public string Vendor { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Number { get; set; } = string.Empty;

    [MaxLength(64)]
    public string SearchVendor { get; set; } = string.Empty;

    [MaxLength(64)]
    public string SearchNumber { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Count { get; set; }

    [MaxLength(128)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }
}
