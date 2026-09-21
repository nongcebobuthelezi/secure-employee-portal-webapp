// Represents a narrow employee request for additional protected access.
namespace SecureEmployeePortal.Data.Entities;

public class AccessRequest
{
    public int Id { get; set; }
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequestedResource { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? ReviewerUserId { get; set; }
    public string? DecisionNote { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
}
