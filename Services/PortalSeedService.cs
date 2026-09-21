// Creates the fixed application roles and an optional local/development administrator.
using Microsoft.AspNetCore.Identity;
using SecureEmployeePortal.Data;

namespace SecureEmployeePortal.Services;

public static class PortalSeedService
{
    public static readonly string[] Roles = ["Employee", "Manager", "Administrator"];

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create the {role} role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }

        var adminEmail = configuration["SeedAdmin:Email"]?.Trim();
        var adminPassword = configuration["SeedAdmin:Password"];

        // Administrator credentials are opt-in and should come from user-secrets or
        // environment configuration rather than source-controlled appsettings files.
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Portal",
                LastName = "Administrator",
                EmployeeNumber = $"ADM-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
                JobTitle = "Security Administrator",
                Department = "Information Security",
                AccountStatus = AccountStatus.Active,
                EmailConfirmed = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to seed administrator: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        // Add the required role before removing any others so an existing seeded account
        // is never left without a usable role if role normalization fails partway through.
        if (!await userManager.IsInRoleAsync(admin, "Administrator"))
        {
            var addResult = await userManager.AddToRoleAsync(admin, "Administrator");
            if (!addResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to assign the Administrator role: {string.Join("; ", addResult.Errors.Select(error => error.Description))}");
            }
        }

        // The portal deliberately uses one clear application role per account.
        var existingRoles = await userManager.GetRolesAsync(admin);
        var rolesToRemove = existingRoles.Where(role => !string.Equals(role, "Administrator", StringComparison.Ordinal)).ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(admin, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to normalize administrator roles: {string.Join("; ", removeResult.Errors.Select(error => error.Description))}");
            }
        }
    }
}
