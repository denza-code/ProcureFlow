using System.ComponentModel.DataAnnotations;

namespace ProcureFlow.Web.ViewModels;

public class RejectPurchaseRequestViewModel
{
    [Required]
    public int PurchaseRequestId { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 5)]
    [Display(Name = "Reason for rejection")]
    public string DecisionNote { get; set; } = string.Empty;
}
