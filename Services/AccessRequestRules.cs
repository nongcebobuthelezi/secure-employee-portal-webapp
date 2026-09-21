// Defines the small, explicit state machine used by employee access requests.
namespace SecureEmployeePortal.Services;

public static class AccessRequestRules
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static bool CanReview(string? currentStatus) =>
        string.Equals(currentStatus, Pending, StringComparison.OrdinalIgnoreCase);

    public static bool IsValidDecision(string? decision) =>
        string.Equals(decision, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(decision, Rejected, StringComparison.OrdinalIgnoreCase);

    public static bool MayReviewerReview(string? reviewerUserId, string? requesterUserId) =>
        !string.IsNullOrWhiteSpace(reviewerUserId)
        && !string.IsNullOrWhiteSpace(requesterUserId)
        && !string.Equals(reviewerUserId, requesterUserId, StringComparison.Ordinal);
}
