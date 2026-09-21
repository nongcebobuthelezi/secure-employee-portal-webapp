// Performs fresh database-backed authorisation checks for sensitive portal actions.
// Route authorisation protects navigation; these checks protect writes if a long-lived
// Blazor session has not yet revalidated after an account or role change.
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Data;

namespace SecureEmployeePortal.Services;

public sealed class PortalAuthorizationService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public async Task<bool> IsActiveUserAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId && user.AccountStatus == AccountStatus.Active);
    }

    public async Task<bool> IsActiveAdministratorAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        return await (
            from user in db.Users.AsNoTracking()
            join userRole in db.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
            join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where user.Id == userId
                && user.AccountStatus == AccountStatus.Active
                && role.NormalizedName == "ADMINISTRATOR"
            select user.Id)
            .AnyAsync();
    }
}
