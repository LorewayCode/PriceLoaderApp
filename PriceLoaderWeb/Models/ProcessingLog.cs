using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceLoaderWeb.Models;

public class ProcessingLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsError { get; set; }
}
