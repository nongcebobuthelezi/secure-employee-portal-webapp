// Implements authenticated employee clock-in/clock-out and attendance summaries.
using System.Data;
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Data.Entities;

namespace SecureEmployeePortal.Services;

public sealed class AttendanceService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    PortalAuthorizationService authorizationService)
{
    public async Task<AttendanceSnapshot> GetSnapshotAsync(string userId, int historyCount = 12)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new AttendanceSnapshot(null, [], 0, 0, 0);
        }

        historyCount = Math.Clamp(historyCount, 1, 100);
        await using var db = await dbFactory.CreateDbContextAsync();

        var history = await db.AttendanceRecords
            .Where(record => record.UserId == userId)
            .OrderByDescending(record => record.ClockInAt)
            .Take(historyCount)
            .AsNoTracking()
            .ToListAsync();

        var openRecord = history.FirstOrDefault(record => record.ClockOutAt is null);
        if (openRecord is null)
        {
            openRecord = await db.AttendanceRecords
                .Where(record => record.UserId == userId && record.ClockOutAt == null)
                .OrderByDescending(record => record.ClockInAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        var now = DateTimeOffset.UtcNow;
        var localNow = now.ToLocalTime();
        var localWeekStart = localNow.Date.AddDays(-(((int)localNow.DayOfWeek + 6) % 7));
        var utcLookback = new DateTimeOffset(localWeekStart, localNow.Offset).ToUniversalTime();

        var weekRecords = await db.AttendanceRecords
            .Where(record => record.UserId == userId && record.ClockInAt >= utcLookback)
            .AsNoTracking()
            .ToListAsync();

        var daysPresent = weekRecords
            .Select(record => record.ClockInAt.ToLocalTime().Date)
            .Where(date => AttendanceRules.IsWorkingDay(date.DayOfWeek))
            .Distinct()
            .Count();

        var completedMinutes = weekRecords.Sum(record => record.TotalMinutes ?? 0);
        var openMinutes = weekRecords
            .Where(record => record.ClockOutAt is null)
            .Sum(record => Math.Max(0, AttendanceRules.CalculateWorkedMinutes(record.ClockInAt, now)));

        var today = localNow.Date;
        var todayRecords = weekRecords
            .Where(record => record.ClockInAt.ToLocalTime().Date == today)
            .ToList();
        var todayMinutes = todayRecords.Sum(record => record.TotalMinutes ?? 0)
            + todayRecords.Where(record => record.ClockOutAt is null)
                .Sum(record => Math.Max(0, AttendanceRules.CalculateWorkedMinutes(record.ClockInAt, now)));

        return new AttendanceSnapshot(
            openRecord,
            history,
            daysPresent,
            completedMinutes + openMinutes,
            todayMinutes);
    }

    public async Task<AttendanceOperationResult> ClockInAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return AttendanceOperationResult.Failed("Your signed-in account could not be verified.");
        }

        if (!await authorizationService.IsActiveUserAsync(userId))
        {
            return AttendanceOperationResult.Failed("Your account must be active before you can clock in.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var hasOpenRecord = await db.AttendanceRecords
            .AnyAsync(record => record.UserId == userId && record.ClockOutAt == null);

        if (!AttendanceRules.CanClockIn(hasOpenRecord))
        {
            return AttendanceOperationResult.Failed("You are already clocked in.");
        }

        db.AttendanceRecords.Add(new AttendanceRecord
        {
            UserId = userId,
            ClockInAt = DateTimeOffset.UtcNow
        });
        db.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = "ClockIn",
            Succeeded = true,
            Description = "Employee clocked in successfully.",
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return AttendanceOperationResult.Succeeded("Clock-in recorded.");
    }

    public async Task<AttendanceOperationResult> ClockOutAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return AttendanceOperationResult.Failed("Your signed-in account could not be verified.");
        }

        // Clock-out is a protected write too. Re-check the account immediately so a
        // recently suspended/disabled session cannot mutate attendance while it waits
        // for the periodic Blazor authentication-state revalidation interval.
        if (!await authorizationService.IsActiveUserAsync(userId))
        {
            return AttendanceOperationResult.Failed("Your account must be active before you can clock out.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var openRecord = await db.AttendanceRecords
            .Where(record => record.UserId == userId && record.ClockOutAt == null)
            .OrderByDescending(record => record.ClockInAt)
            .FirstOrDefaultAsync();

        if (!AttendanceRules.CanClockOut(openRecord is not null) || openRecord is null)
        {
            return AttendanceOperationResult.Failed("There is no active clock-in to close.");
        }

        var now = DateTimeOffset.UtcNow;
        openRecord.ClockOutAt = now;
        openRecord.TotalMinutes = AttendanceRules.CalculateWorkedMinutes(openRecord.ClockInAt, now);
        db.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = "ClockOut",
            Succeeded = true,
            Description = "Employee clocked out successfully.",
            OccurredAt = now
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return AttendanceOperationResult.Succeeded("Clock-out recorded.");
    }
}

public sealed record AttendanceSnapshot(
    AttendanceRecord? OpenRecord,
    IReadOnlyList<AttendanceRecord> History,
    int DaysPresentThisWeek,
    int TotalMinutesThisWeek,
    int TodayMinutes);

public sealed record AttendanceOperationResult(bool Success, string Message)
{
    public static AttendanceOperationResult Succeeded(string message) => new(true, message);
    public static AttendanceOperationResult Failed(string message) => new(false, message);
}
