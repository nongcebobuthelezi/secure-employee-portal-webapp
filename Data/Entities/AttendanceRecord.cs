// Stores one employee clock-in/clock-out session.
namespace SecureEmployeePortal.Data.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset ClockInAt { get; set; }
    public DateTimeOffset? ClockOutAt { get; set; }
    public int? TotalMinutes { get; set; }
}
