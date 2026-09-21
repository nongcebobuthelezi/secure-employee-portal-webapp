// Captures the email submitted to the password-recovery workflow.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class ForgotPasswordInputModel
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
