using System.ComponentModel.DataAnnotations;

namespace ProcureFlow.Web.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Supplier name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "VAT / OIB number")]
    public string? VatNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "Contact person")]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }
}