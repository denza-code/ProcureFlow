using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureFlow.Web.ViewModels;

public class AddPurchaseRequestItemViewModel
{
    [Required]
    public int PurchaseRequestId { get; set; }

    [Required]
    [Display(Name = "Catalog item")]
    public int? CatalogItemId { get; set; }

    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; } = 1;

    public IEnumerable<SelectListItem> CatalogItems { get; set; } = Array.Empty<SelectListItem>();
}
