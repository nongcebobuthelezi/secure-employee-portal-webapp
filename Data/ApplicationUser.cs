// Defines the employee account used by ASP.NET Core Identity
// throughout the Secure Employee Portal.
using Microsoft.AspNetCore.Identity;

namespace SecureEmployeePortal.Data;

/// <summary>
/// Represents one authenticated employee account in the portal.
/// IdentityUser already provides the standard account and security fields,
/// including email, username, password hash, and lockout information.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Stores the employee's first name.
    [PersonalData]
    public string FirstName { get; set; } = string.Empty;

    // Stores the employee's last name.
    [PersonalData]
    public string LastName { get; set; } = string.Empty;
}