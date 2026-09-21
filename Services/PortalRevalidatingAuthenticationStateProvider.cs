// Revalidates long-lived Blazor server sessions against account status and security stamp.
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SecureEmployeePortal.Data;

namespace SecureEmployeePortal.Services;

public sealed class PortalRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> identityOptions)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(authenticationState.User);

        if (user is null || !AccountAccessRules.MaySignIn(user.AccountStatus))
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var principalStamp = authenticationState.User.FindFirstValue(
            identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        var currentStamp = await userManager.GetSecurityStampAsync(user);

        return principalStamp == currentStamp;
    }
}
