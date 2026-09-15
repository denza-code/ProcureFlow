using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProcureFlow.Web.ViewModels;

public class CreateInvoiceViewModel
{
    [Required]
    [Display(Name = "Purchase order")]
    public int? PurchaseOrderId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Invoice number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Invoice date")]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

    [Required]
    [Range(0.01, 99999999.99)]
    [Display(Name = "Invoice amount (€)")]
    public decimal Amount { get; set; }

    public IEnumerable<SelectListItem> PurchaseOrders { get; set; } = Array.Empty<SelectListItem>();
}
