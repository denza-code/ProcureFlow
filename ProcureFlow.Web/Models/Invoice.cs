using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProcureFlow.Web.Models.Enums;

namespace ProcureFlow.Web.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Invoice number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Purchase order")]
    public int PurchaseOrderId { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Invoice date")]
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime DueDate { get; set; }

    [Required]
    [Range(0.01, 99999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Invoice amount (€)")]
    public decimal Amount { get; set; }

    [Display(Name = "Status")]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Received;

    [Display(Name = "Received")]
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

    [Display(Name = "Paid")]
    public DateTime? PaidAtUtc { get; set; }
}
