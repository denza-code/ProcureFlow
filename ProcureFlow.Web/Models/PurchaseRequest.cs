using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using ProcureFlow.Web.Models.Enums;

namespace ProcureFlow.Web.Models;

public class PurchaseRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Request number")]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    [Display(Name = "Business justification")]
    public string BusinessJustification { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;

    [Required]
    public string RequesterUserId { get; set; } = string.Empty;

    public IdentityUser? RequesterUser { get; set; }

    [Display(Name = "Created")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Display(Name = "Submitted")]
    public DateTime? SubmittedAtUtc { get; set; }

    [Display(Name = "Decided")]
    public DateTime? DecidedAtUtc { get; set; }

    public string? DecisionByUserId { get; set; }

    public IdentityUser? DecisionByUser { get; set; }

    [StringLength(1000)]
    [Display(Name = "Decision note")]
    public string? DecisionNote { get; set; }

    public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
}
