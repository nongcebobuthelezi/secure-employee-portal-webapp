// Defines the Entity Framework Core database context used to store
// Secure Employee Portal users, roles, logins, and security information.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SecureEmployeePortal.Data;

/// <summary>
/// Provides the database gateway for ASP.NET Core Identity.
/// ApplicationUser tells Identity which customised employee-account
/// type should be stored in the database.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Receives the database configuration that will later be registered
    // in Program.cs and passes it to IdentityDbContext.
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}