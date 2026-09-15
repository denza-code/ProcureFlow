using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureFlow.Web.Models;

public class PurchaseOrderItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseOrderId { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Item name")]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Unit of measure")]
    public string UnitOfMeasure { get; set; } = "pcs";

    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Unit price (€)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal Total => Quantity * UnitPrice;
}
