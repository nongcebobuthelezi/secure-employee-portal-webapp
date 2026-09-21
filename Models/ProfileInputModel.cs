// Limits self-service profile editing to explicitly permitted fields.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class ProfileInputModel
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [StringLength(40)]
    public string? PhoneNumber { get; set; }
}
