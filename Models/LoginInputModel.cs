// Captures and validates credentials submitted by the login page.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class LoginInputModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
