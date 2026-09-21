// Captures a secure self-service password change.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class ChangePasswordInputModel
{
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new passwords do not match.")]
    [StringLength(100)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
