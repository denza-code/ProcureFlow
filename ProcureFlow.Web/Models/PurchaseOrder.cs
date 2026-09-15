using System.ComponentModel.DataAnnotations;
using ProcureFlow.Web.Models.Enums;

namespace ProcureFlow.Web.Models;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Purchase order number")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int PurchaseRequestId { get; set; }

    public PurchaseRequest? PurchaseRequest { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    [Display(Name = "Status")]
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    [Display(Name = "Created")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Display(Name = "Sent")]
    public DateTime? SentAtUtc { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
