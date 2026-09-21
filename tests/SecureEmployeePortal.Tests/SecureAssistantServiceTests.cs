using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Services;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class SecureAssistantServiceTests
{
    private readonly SecureAssistantService _service = new();

    [TestMethod]
    public void GetReply_ShortGreeting_ReturnsAssistantWelcome()
    {
        var reply = _service.GetReply("hi");
        Assert.IsNull(reply.ActionHref);
        StringAssert.StartsWith(reply.Message, "Hi!");
    }

    [TestMethod]
    public void GetReply_PasswordQuestion_PointsToSecurity()
    {
        var reply = _service.GetReply("Where can I change my password?");
        Assert.AreEqual("/security", reply.ActionHref);
        StringAssert.Contains(reply.Message, "password");
    }

    [TestMethod]
    public void GetReply_AttendanceQuestion_PointsToAttendance()
    {
        var reply = _service.GetReply("How do I clock in?");
        Assert.AreEqual("/attendance", reply.ActionHref);
        StringAssert.Contains(reply.Message, "attendance");
    }

    [TestMethod]
    public void GetReply_CurrentClockingStatus_UsesDashboardContext()
    {
        var reply = _service.GetReply("Am I clocked in?", isClockedIn: true);
        Assert.AreEqual("/attendance", reply.ActionHref);
        StringAssert.Contains(reply.Message, "currently clocked in");
    }

    [TestMethod]
    public void GetReply_TodaysHours_UsesDashboardContext()
    {
        var reply = _service.GetReply("How many hours have I worked today?", todayMinutes: 95);
        Assert.AreEqual("/attendance", reply.ActionHref);
        StringAssert.Contains(reply.Message, "01:35");
    }

    [TestMethod]
    public void GetReply_AccessQuestion_PointsToRequests()
    {
        var reply = _service.GetReply("How do I request protected access?");
        Assert.AreEqual("/requests", reply.ActionHref);
        StringAssert.Contains(reply.Message, "Access Requests");
    }

    [TestMethod]
    public void GetReply_ProfileQuestion_PointsToProfile()
    {
        var reply = _service.GetReply("Where do I update my profile details?");
        Assert.AreEqual("/profile", reply.ActionHref);
    }

    [TestMethod]
    public void GetReply_DisabledAccount_ExplainsStatusAndPointsToActivity()
    {
        var reply = _service.GetReply("What is my account status?", accountStatus: AccountStatus.Disabled);
        Assert.AreEqual("/account-activity", reply.ActionHref);
        StringAssert.Contains(reply.Message, "disabled");
    }

    [TestMethod]
    public void GetReply_CurrentRole_UsesAuthenticatedRoleInsteadOfAccessRequestFallback()
    {
        var reply = _service.GetReply("What is my role?", "Manager");
        Assert.AreEqual("/account-activity", reply.ActionHref);
        StringAssert.Contains(reply.Message, "Manager");
    }

    [TestMethod]
    public void GetReply_AdministratorQuestion_UsesAdminDestinationForAdministrator()
    {
        var reply = _service.GetReply("Where is user management?", "Administrator");
        Assert.AreEqual("/admin", reply.ActionHref);
        StringAssert.Contains(reply.Message, "Security Admin");
    }

    [TestMethod]
    public void GetReply_NonAdministratorQuestion_DoesNotExposeAdminDestination()
    {
        var reply = _service.GetReply("Where is user management?", "Employee");
        Assert.AreEqual("/requests", reply.ActionHref);
        Assert.AreNotEqual("/admin", reply.ActionHref);
    }

    [TestMethod]
    public void GetReply_EmptyQuestion_ReturnsSupportedTopicPrompt()
    {
        var reply = _service.GetReply("   ");
        Assert.IsNull(reply.ActionHref);
        StringAssert.Contains(reply.Message, "password");
        StringAssert.Contains(reply.Message, "attendance");
    }

    [TestMethod]
    public void GetReply_OverlongQuestion_ReturnsBoundedLengthGuidance()
    {
        var reply = _service.GetReply(new string('a', 241));
        Assert.IsNull(reply.ActionHref);
        StringAssert.Contains(reply.Message, "240");
    }

    [TestMethod]
    public void GetReply_UnknownQuestion_ProvidesSupportedTopicsWithoutInventingAnAnswer()
    {
        var reply = _service.GetReply("Can you order me lunch?");
        Assert.IsNull(reply.ActionHref);
        StringAssert.Contains(reply.Message, "account security");
        StringAssert.Contains(reply.Message, "attendance");
    }
}
