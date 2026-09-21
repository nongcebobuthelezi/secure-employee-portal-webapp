// Verifies attendance rules independently from the Blazor interface and database provider.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureEmployeePortal.Services;

namespace SecureEmployeePortal.Tests;

[TestClass]
public sealed class AttendanceRulesTests
{
    [TestMethod]
    public void CanClockIn_WhenNoOpenRecord_ReturnsTrue()
    {
        Assert.IsTrue(AttendanceRules.CanClockIn(hasOpenRecord: false));
    }

    [TestMethod]
    public void CanClockIn_WhenAlreadyClockedIn_ReturnsFalse()
    {
        Assert.IsFalse(AttendanceRules.CanClockIn(hasOpenRecord: true));
    }

    [TestMethod]
    public void CanClockOut_WhenOpenRecordExists_ReturnsTrue()
    {
        Assert.IsTrue(AttendanceRules.CanClockOut(hasOpenRecord: true));
    }

    [TestMethod]
    public void CanClockOut_WhenNoOpenRecord_ReturnsFalse()
    {
        Assert.IsFalse(AttendanceRules.CanClockOut(hasOpenRecord: false));
    }

    [TestMethod]
    public void IsWorkingDay_ForWeekday_ReturnsTrue()
    {
        Assert.IsTrue(AttendanceRules.IsWorkingDay(DayOfWeek.Monday));
        Assert.IsTrue(AttendanceRules.IsWorkingDay(DayOfWeek.Friday));
    }

    [TestMethod]
    public void IsWorkingDay_ForWeekend_ReturnsFalse()
    {
        Assert.IsFalse(AttendanceRules.IsWorkingDay(DayOfWeek.Saturday));
        Assert.IsFalse(AttendanceRules.IsWorkingDay(DayOfWeek.Sunday));
    }

    [TestMethod]
    public void CalculateWorkedMinutes_ReturnsElapsedMinutes()
    {
        var clockIn = new DateTimeOffset(2026, 8, 25, 8, 0, 0, TimeSpan.Zero);
        var clockOut = clockIn.AddHours(2).AddMinutes(35);

        Assert.AreEqual(155, AttendanceRules.CalculateWorkedMinutes(clockIn, clockOut));
    }

    [TestMethod]
    public void CalculateWorkedMinutes_WhenClockOutPrecedesClockIn_Throws()
    {
        var clockIn = DateTimeOffset.UtcNow;
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            AttendanceRules.CalculateWorkedMinutes(clockIn, clockIn.AddMinutes(-1)));
    }
}
