// Captures the minimum information an administrator needs to create an employee account.
using System.ComponentModel.DataAnnotations;

namespace SecureEmployeePortal.Models;

public sealed class CreateEmployeeInputModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Work email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    [Display(Name = "Employee number")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary password")]
    public string TemporaryPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(TemporaryPassword), ErrorMessage = "The temporary passwords do not match.")]
    [StringLength(100)]
    [Display(Name = "Confirm temporary password")]
    public string ConfirmTemporaryPassword { get; set; } = string.Empty;
}
