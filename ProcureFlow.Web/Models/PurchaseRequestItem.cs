using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureFlow.Web.Models;

public class PurchaseRequestItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseRequestId { get; set; }

    public PurchaseRequest? PurchaseRequest { get; set; }

    [Required]
    [Display(Name = "Catalog item")]
    public int CatalogItemId { get; set; }

    public CatalogItem? CatalogItem { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, 999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Estimated unit price (€)")]
    public decimal EstimatedUnitPrice { get; set; }

    [NotMapped]
    [Display(Name = "Estimated total (€)")]
    public decimal EstimatedTotal => Quantity * EstimatedUnitPrice;
}