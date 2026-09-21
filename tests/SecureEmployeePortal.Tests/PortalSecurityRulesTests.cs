// Verifies redirect targets remain inside the Secure Employee Portal.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Services;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class PortalSecurityRulesTests
{
    [TestMethod]
    [DataRow("/dashboard")]
    [DataRow("/profile?from=login")]
    [DataRow("/admin/users")]
    public void IsSafeLocalReturnUrl_AllowsPortalPaths(string returnUrl)
    {
        Assert.IsTrue(PortalSecurityRules.IsSafeLocalReturnUrl(returnUrl));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("dashboard")]
    [DataRow("https://example.com")]
    [DataRow("//example.com")]
    [DataRow("/\\example.com")]
    [DataRow("/dashboard\\admin")]
    [DataRow("/dashboard\r\nLocation:https://example.com")]
    public void IsSafeLocalReturnUrl_RejectsExternalOrMalformedTargets(string? returnUrl)
    {
        Assert.IsFalse(PortalSecurityRules.IsSafeLocalReturnUrl(returnUrl));
    }
}
