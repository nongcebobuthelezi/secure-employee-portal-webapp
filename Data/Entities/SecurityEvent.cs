// Stores authentication and account-security events without storing secrets.
namespace SecureEmployeePortal.Data.Entities;

public class SecurityEvent
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
