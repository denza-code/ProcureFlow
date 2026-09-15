using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureFlow.Web.ViewModels;

public class CreatePurchaseOrderViewModel
{
    [Required]
    [Display(Name = "Approved purchase request")]
    public int? PurchaseRequestId { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    public IEnumerable<SelectListItem> PurchaseRequests { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Suppliers { get; set; } = Array.Empty<SelectListItem>();
}
