// Defines the temporary information submitted through
// the Secure Employee Portal registration form.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

/// <summary>
/// Represents the information entered while creating an employee account.
/// This model is used for form binding and validation only.
/// It is not stored directly as an Identity database record.
/// </summary>
public sealed class RegisterInputModel
{
    // Collects the employee's first name.
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(
        100,
        ErrorMessage = "First name cannot exceed 100 characters.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    // Collects the employee's last name.
    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(
        100,
        ErrorMessage = "Last name cannot exceed 100 characters.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    // Collects and validates the employee's work email address.
    [Required(ErrorMessage = "Work email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid work email address.")]
    [Display(Name = "Work email")]
    public string WorkEmail { get; set; } = string.Empty;

    // Collects the new account password.
    // Identity will perform the final password-policy checks.
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 100 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Confirms that the employee entered the intended password.
    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Requires acknowledgement of the portal policies
    // before the account may be created.
    [Range(
        typeof(bool),
        "true",
        "true",
        ErrorMessage = "You must accept the Terms of Service and Privacy Policy.")]
    public bool AcceptTerms { get; set; }
}