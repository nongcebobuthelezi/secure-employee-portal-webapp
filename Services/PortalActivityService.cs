// Centralises security-event and audit-log recording with short-lived EF contexts.
// Activity recording is deliberately best-effort: an observability/database logging
// failure is written to application logs but must not undo an account action that has
// already completed in Identity or another business workflow.
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Data.Entities;

namespace SecureEmployeePortal.Services;

public sealed class PortalActivityService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<PortalActivityService> logger)
{
    public async Task<bool> RecordSecurityEventAsync(
        string? userId,
        string eventType,
        bool succeeded,
        string description)
    {
        try
        {
            await using var dbContext = await dbFactory.CreateDbContextAsync();
            dbContext.SecurityEvents.Add(new SecurityEvent
            {
                UserId = userId,
                EventType = Bound(eventType, 80, "UnknownEvent"),
                Succeeded = succeeded,
                Description = Bound(description, 500, "No event details were supplied."),
                OccurredAt = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Security-event recording failed for event type {EventType} and user {UserId}.",
                Bound(eventType, 80, "UnknownEvent"),
                userId ?? "unauthenticated");
            return false;
        }
    }

    public async Task<bool> RecordAuditAsync(
        string actorUserId,
        string? targetUserId,
        string actionType,
        string? previousValue,
        string? newValue,
        bool succeeded = true)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            logger.LogWarning("Audit recording skipped because the actor user id was missing for action {ActionType}.", actionType);
            return false;
        }

        try
        {
            await using var dbContext = await dbFactory.CreateDbContextAsync();
            dbContext.AuditLogs.Add(new AuditLog
            {
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                ActionType = Bound(actionType, 120, "UnknownAction"),
                PreviousValue = BoundNullable(previousValue, 500),
                NewValue = BoundNullable(newValue, 500),
                Succeeded = succeeded,
                OccurredAt = DateTimeOffset.UtcNow
            });

            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Audit recording failed for action {ActionType}, actor {ActorUserId}, target {TargetUserId}.",
                Bound(actionType, 120, "UnknownAction"),
                actorUserId,
                targetUserId ?? "none");
            return false;
        }
    }

    private static string Bound(string? value, int maximumLength, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }

    private static string? BoundNullable(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}
