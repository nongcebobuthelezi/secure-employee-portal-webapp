// Records important administrative changes for accountability.
namespace SecureEmployeePortal.Data.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string? TargetUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public bool Succeeded { get; set; } = true;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
