// Defines the employee account used by ASP.NET Core Identity throughout the portal.
using Microsoft.AspNetCore.Identity;

namespace SecureEmployeePortal.Data;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string FirstName { get; set; } = string.Empty;

    [PersonalData]
    public string LastName { get; set; } = string.Empty;

    [PersonalData]
    public string EmployeeNumber { get; set; } = string.Empty;

    [PersonalData]
    public string JobTitle { get; set; } = "Employee";

    [PersonalData]
    public string Department { get; set; } = "Operations";

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset? LastPasswordChangeAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
