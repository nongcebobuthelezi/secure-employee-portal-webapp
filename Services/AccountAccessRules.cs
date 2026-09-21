// Keeps small account-access decisions explicit and independently testable.
using SecureEmployeePortal.Data;

namespace SecureEmployeePortal.Services;

public static class AccountAccessRules
{
    public static bool MaySignIn(AccountStatus status) => status == AccountStatus.Active;

    public static bool MayAdministratorChangeTarget(string? actorUserId, string? targetUserId) =>
        !string.IsNullOrWhiteSpace(actorUserId)
        && !string.IsNullOrWhiteSpace(targetUserId)
        && !string.Equals(actorUserId, targetUserId, StringComparison.Ordinal);

    // Keep role assignment inside the portal's deliberately small, documented role model.
    // A stray database role must never become assignable merely because it exists in Identity.
    public static bool IsKnownRole(string? role) =>
        !string.IsNullOrWhiteSpace(role)
        && PortalSeedService.Roles.Contains(role, StringComparer.Ordinal);
}
