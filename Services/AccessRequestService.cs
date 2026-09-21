// Implements the narrow employee access-request workflow and administrator review actions.
using System.Data;
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Data.Entities;
using SecureEmployeePortal.Models;

namespace SecureEmployeePortal.Services;

public sealed class AccessRequestService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    PortalAuthorizationService authorizationService)
{
    public async Task<IReadOnlyList<AccessRequest>> GetForEmployeeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return [];
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.AccessRequests
            .Where(request => request.RequesterUserId == userId)
            .OrderByDescending(request => request.RequestedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AccessRequest>> GetAllAsync(string? status = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.AccessRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(request => request.Status == status);
        }

        return await query
            .OrderByDescending(request => request.RequestedAt)
            .ToListAsync();
    }

    public async Task<AccessRequestOperationResult> SubmitAsync(string userId, AccessRequestInputModel input)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return AccessRequestOperationResult.Failed("Your signed-in account could not be verified.");
        }

        if (!await authorizationService.IsActiveUserAsync(userId))
        {
            return AccessRequestOperationResult.Failed("Your account must be active before you can submit an access request.");
        }

        var resource = input.RequestedResource?.Trim() ?? string.Empty;
        var reason = input.Reason?.Trim() ?? string.Empty;

        if (resource.Length is < 1 or > 160)
        {
            return AccessRequestOperationResult.Failed("Enter a requested resource of 160 characters or fewer.");
        }

        if (reason.Length is < 10 or > 1000)
        {
            return AccessRequestOperationResult.Failed("Enter a business reason between 10 and 1000 characters.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var duplicate = await db.AccessRequests.AnyAsync(request =>
            request.RequesterUserId == userId
            && request.Status == AccessRequestRules.Pending
            && request.RequestedResource == resource);

        if (duplicate)
        {
            return AccessRequestOperationResult.Failed("You already have a pending request for this resource.");
        }

        db.AccessRequests.Add(new AccessRequest
        {
            RequesterUserId = userId,
            RequestedResource = resource,
            Reason = reason,
            Status = AccessRequestRules.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        });
        db.SecurityEvents.Add(new SecurityEvent
        {
            UserId = userId,
            EventType = "AccessRequestSubmitted",
            Succeeded = true,
            Description = $"Access request submitted for {resource}.",
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return AccessRequestOperationResult.Succeeded("Access request submitted for review.");
    }

    public async Task<AccessRequestOperationResult> ReviewAsync(
        int requestId,
        string reviewerUserId,
        string decision,
        string? decisionNote)
    {
        if (string.IsNullOrWhiteSpace(reviewerUserId))
        {
            return AccessRequestOperationResult.Failed("Your administrator session could not be verified.");
        }

        if (!await authorizationService.IsActiveAdministratorAsync(reviewerUserId))
        {
            return AccessRequestOperationResult.Failed("Your administrator access is no longer active. Sign in again before reviewing requests.");
        }

        if (!AccessRequestRules.IsValidDecision(decision))
        {
            return AccessRequestOperationResult.Failed("Choose Approve or Reject.");
        }

        var normalizedDecision = string.Equals(decision, AccessRequestRules.Approved, StringComparison.OrdinalIgnoreCase)
            ? AccessRequestRules.Approved
            : AccessRequestRules.Rejected;
        var normalizedNote = string.IsNullOrWhiteSpace(decisionNote) ? null : decisionNote.Trim();
        if (normalizedNote?.Length > 1000)
        {
            return AccessRequestOperationResult.Failed("Decision notes cannot exceed 1000 characters.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var request = await db.AccessRequests.SingleOrDefaultAsync(item => item.Id == requestId);

        if (request is null)
        {
            return AccessRequestOperationResult.Failed("The access request could not be found.");
        }

        if (!AccessRequestRules.CanReview(request.Status))
        {
            return AccessRequestOperationResult.Failed("This request has already been reviewed.");
        }

        if (!AccessRequestRules.MayReviewerReview(reviewerUserId, request.RequesterUserId))
        {
            return AccessRequestOperationResult.Failed("Administrators cannot approve or reject their own access requests.");
        }

        var previous = request.Status;
        var occurredAt = DateTimeOffset.UtcNow;
        request.Status = normalizedDecision;
        request.DecisionNote = normalizedNote;
        request.ReviewerUserId = reviewerUserId;
        request.ReviewedAt = occurredAt;

        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = reviewerUserId,
            TargetUserId = request.RequesterUserId,
            ActionType = "AccessRequestReviewed",
            PreviousValue = previous,
            NewValue = normalizedDecision,
            Succeeded = true,
            OccurredAt = occurredAt
        });
        db.SecurityEvents.Add(new SecurityEvent
        {
            UserId = request.RequesterUserId,
            EventType = $"AccessRequest{normalizedDecision}",
            Succeeded = true,
            Description = $"Access request for {request.RequestedResource} was {normalizedDecision.ToLowerInvariant()}.",
            OccurredAt = occurredAt
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return AccessRequestOperationResult.Succeeded($"Request {normalizedDecision.ToLowerInvariant()}.");
    }
}

public sealed record AccessRequestOperationResult(bool Success, string Message)
{
    public static AccessRequestOperationResult Succeeded(string message) => new(true, message);
    public static AccessRequestOperationResult Failed(string message) => new(false, message);
}
