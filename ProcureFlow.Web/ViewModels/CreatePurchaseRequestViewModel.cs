using System.ComponentModel.DataAnnotations;

namespace ProcureFlow.Web.ViewModels;

public class CreatePurchaseRequestViewModel
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Request title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    [Display(Name = "Business justification")]
    public string BusinessJustification { get; set; } = string.Empty;
}
