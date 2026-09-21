// Contains attendance rules that can be tested independently of the UI.
namespace SecureEmployeePortal.Services;

public static class AttendanceRules
{
    public static bool CanClockIn(bool hasOpenRecord) => !hasOpenRecord;
    public static bool CanClockOut(bool hasOpenRecord) => hasOpenRecord;

    public static bool IsWorkingDay(DayOfWeek day) =>
        day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;

    public static int CalculateWorkedMinutes(DateTimeOffset clockIn, DateTimeOffset clockOut)
    {
        if (clockOut < clockIn)
        {
            throw new ArgumentOutOfRangeException(nameof(clockOut), "Clock-out cannot be earlier than clock-in.");
        }

        return (int)Math.Round((clockOut - clockIn).TotalMinutes, MidpointRounding.AwayFromZero);
    }
}
