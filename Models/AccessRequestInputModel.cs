// Captures a narrow request for access to a protected resource.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class AccessRequestInputModel
{
    [Required]
    [StringLength(160)]
    [Display(Name = "Requested resource")]
    public string RequestedResource { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Reason { get; set; } = string.Empty;
}
