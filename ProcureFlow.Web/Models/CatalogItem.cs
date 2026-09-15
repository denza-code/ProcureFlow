using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureFlow.Web.Models;

public class CatalogItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Item name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Item code")]
    public string? ItemCode { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Estimated unit price (€)")]
    public decimal EstimatedUnitPrice { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Unit of measure")]
    public string UnitOfMeasure { get; set; } = "pcs";
}