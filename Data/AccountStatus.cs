// Defines the lifecycle states that control whether an employee account may sign in.
namespace SecureEmployeePortal.Data;

public enum AccountStatus
{
    Active = 0,
    Suspended = 1,
    Disabled = 2
}
