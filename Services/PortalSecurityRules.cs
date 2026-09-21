// Keeps small security-sensitive URL decisions explicit and independently testable.
namespace SecureEmployeePortal.Services;

public static class PortalSecurityRules
{
    // Only application-local absolute paths are accepted after authentication.
    // This blocks protocol-relative and backslash-based destinations that could
    // otherwise be interpreted as external redirects by a browser or proxy.
    public static bool IsSafeLocalReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl[0] != '/')
        {
            return false;
        }

        return !returnUrl.StartsWith("//", StringComparison.Ordinal)
            && !returnUrl.Contains('\\')
            && !returnUrl.Contains('\r')
            && !returnUrl.Contains('\n');
    }
}
