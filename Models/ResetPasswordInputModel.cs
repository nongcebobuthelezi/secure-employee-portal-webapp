// Captures a token-backed password reset.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class ResetPasswordInputModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [StringLength(100)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
