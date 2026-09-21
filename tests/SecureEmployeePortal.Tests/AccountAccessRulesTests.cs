// Verifies account-status and administrator self-protection decisions.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Services;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class AccountAccessRulesTests
{
    [TestMethod]
    public void MaySignIn_OnlyAllowsActiveAccounts()
    {
        Assert.IsTrue(AccountAccessRules.MaySignIn(AccountStatus.Active));
        Assert.IsFalse(AccountAccessRules.MaySignIn(AccountStatus.Suspended));
        Assert.IsFalse(AccountAccessRules.MaySignIn(AccountStatus.Disabled));
    }

    [TestMethod]
    public void MayAdministratorChangeTarget_RejectsSelfChange()
    {
        Assert.IsFalse(AccountAccessRules.MayAdministratorChangeTarget("admin-1", "admin-1"));
    }

    [TestMethod]
    public void MayAdministratorChangeTarget_AllowsDifferentValidUser()
    {
        Assert.IsTrue(AccountAccessRules.MayAdministratorChangeTarget("admin-1", "employee-2"));
    }

    [TestMethod]
    public void IsKnownRole_AllowsOnlyDocumentedPortalRoles()
    {
        Assert.IsTrue(AccountAccessRules.IsKnownRole("Employee"));
        Assert.IsTrue(AccountAccessRules.IsKnownRole("Manager"));
        Assert.IsTrue(AccountAccessRules.IsKnownRole("Administrator"));
        Assert.IsFalse(AccountAccessRules.IsKnownRole("SuperAdmin"));
        Assert.IsFalse(AccountAccessRules.IsKnownRole(string.Empty));
        Assert.IsFalse(AccountAccessRules.IsKnownRole(null));
    }

    [TestMethod]
    public void MayAdministratorChangeTarget_RejectsMissingIdentity()
    {
        Assert.IsFalse(AccountAccessRules.MayAdministratorChangeTarget(null, "employee-2"));
        Assert.IsFalse(AccountAccessRules.MayAdministratorChangeTarget("admin-1", null));
    }
}
