// Verifies the small state machine used by administrator access-request decisions.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Services;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class AccessRequestRulesTests
{
    [TestMethod]
    public void CanReview_OnlyAllowsPendingRequests()
    {
        Assert.IsTrue(AccessRequestRules.CanReview(AccessRequestRules.Pending));
        Assert.IsFalse(AccessRequestRules.CanReview(AccessRequestRules.Approved));
        Assert.IsFalse(AccessRequestRules.CanReview(AccessRequestRules.Rejected));
    }

    [TestMethod]
    public void IsValidDecision_AllowsApproveAndRejectOnly()
    {
        Assert.IsTrue(AccessRequestRules.IsValidDecision(AccessRequestRules.Approved));
        Assert.IsTrue(AccessRequestRules.IsValidDecision(AccessRequestRules.Rejected));
        Assert.IsFalse(AccessRequestRules.IsValidDecision(AccessRequestRules.Pending));
        Assert.IsFalse(AccessRequestRules.IsValidDecision("Escalated"));
    }

    [TestMethod]
    public void MayReviewerReview_RejectsSelfReviewAndMissingIdentities()
    {
        Assert.IsTrue(AccessRequestRules.MayReviewerReview("admin-1", "employee-2"));
        Assert.IsFalse(AccessRequestRules.MayReviewerReview("admin-1", "admin-1"));
        Assert.IsFalse(AccessRequestRules.MayReviewerReview(null, "employee-2"));
        Assert.IsFalse(AccessRequestRules.MayReviewerReview("admin-1", null));
    }
}
