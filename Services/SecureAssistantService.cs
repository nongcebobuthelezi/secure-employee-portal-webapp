// Provides deterministic, in-app support guidance for the Secure Employee Portal.
// This assistant intentionally uses portal-owned rules rather than an external AI service,
// so it remains reliable, testable, private, and usable in a local portfolio demo.
using SecureEmployeePortal.Data;

namespace SecureEmployeePortal.Services;

public sealed class SecureAssistantService
{
    public SecureAssistantReply GetReply(
        string? message,
        string currentRole = "Employee",
        AccountStatus accountStatus = AccountStatus.Active,
        bool? isClockedIn = null,
        int? todayMinutes = null)
    {
        var normalized = (message ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            return new(
                "Type a question about your account, password, attendance, access requests, profile, or recent activity.");
        }

        if (normalized.Length > 240)
        {
            return new(
                "Please keep your question under 240 characters so I can match it to a supported portal topic.");
        }

        var text = normalized.ToLowerInvariant();

        if (IsGreeting(text))
        {
            return new(
                "Hi! I can help with account security, attendance, profile updates, access requests, account activity, and role-based administration. Ask a question or choose one of the quick prompts below.");
        }

        if (ContainsAny(text, "what is my role", "what's my role", "whats my role", "my current role", "which role", "current role"))
        {
            return new(
                $"Your current portal role is {currentRole}. Role-based permissions control which protected areas you can open.",
                "Review account activity",
                "/account-activity");
        }

        if (ContainsAny(text, "account status", "is my account active", "my account active", "suspended", "disabled"))
        {
            var statusText = accountStatus switch
            {
                AccountStatus.Active => "Your account is currently active.",
                AccountStatus.Suspended => "Your account is currently suspended and may require administrator review.",
                AccountStatus.Disabled => "Your account is currently disabled and requires administrator action.",
                _ => "You can review your current account status in the portal."
            };

            return new(
                $"{statusText} Account Activity shows recent sign-ins and security-related account events.",
                "Open Account Activity",
                "/account-activity");
        }

        if (ContainsAny(text, "am i clocked in", "clocked in now", "clocking status", "am i clocked out"))
        {
            var statusText = isClockedIn switch
            {
                true => "You are currently clocked in.",
                false => "You are currently clocked out.",
                null => "Open My Attendance to check your current clocking status."
            };

            return new(
                $"{statusText} You can clock in or out and review your records from My Attendance.",
                "Open My Attendance",
                "/attendance");
        }

        if (ContainsAny(text, "hours today", "worked today", "today's hours", "todays hours"))
        {
            var hoursText = todayMinutes.HasValue
                ? $"You have {FormatMinutes(todayMinutes.Value)} recorded today."
                : "Your recorded hours for today are shown in My Attendance.";

            return new(
                $"{hoursText} Open My Attendance for the full record.",
                "Open My Attendance",
                "/attendance");
        }

        if (ContainsAny(text, "password", "forgot", "reset", "security", "secure", "login", "sign in", "signin"))
        {
            return new(
                "You can review account security and change your password from My Security. If you cannot sign in, use the password-recovery link on the sign-in page.",
                "Open My Security",
                "/security");
        }

        if (ContainsAny(text, "attendance", "clock in", "clock-in", "clock out", "clock-out", "timesheet", "hours", "worked"))
        {
            return new(
                "Use My Attendance to clock in or out and review your recent attendance history. Your dashboard also shows today's recorded hours and this week's attendance summary.",
                "Open My Attendance",
                "/attendance");
        }

        if (ContainsAny(text, "access request", "request access", "protected access", "permission", "permissions", "resource", "role", "roles"))
        {
            var roleNote = string.Equals(currentRole, "Administrator", StringComparison.OrdinalIgnoreCase)
                ? " As an administrator, you can also review employee requests from Security Admin."
                : string.Empty;

            return new(
                "Use Access Requests to request access to a protected resource and track the decision." + roleNote,
                "Open Access Requests",
                "/requests");
        }

        if (ContainsAny(text, "profile", "personal information", "personal info", "name", "phone", "department", "job title", "details"))
        {
            return new(
                "Open My Profile to review your employee identity information and update the personal details that are editable in the portal.",
                "Open My Profile",
                "/profile");
        }

        if (ContainsAny(text, "activity", "recent sign", "history", "audit", "account event"))
        {
            return new(
                "Account Activity shows recent sign-ins and security-related account events connected to your employee identity.",
                "Open Account Activity",
                "/account-activity");
        }

        if (ContainsAny(text, "admin", "administrator", "user management", "manage users"))
        {
            if (string.Equals(currentRole, "Administrator", StringComparison.OrdinalIgnoreCase))
            {
                return new(
                    "Your Security Admin area includes user management, roles and permissions, access-request review, security events, and audit logs.",
                    "Open Security Admin",
                    "/admin");
            }

            return new(
                "Administrator tools are restricted by role. If you need an account, role, or access change, submit an access request or contact an authorised administrator.",
                "Open Access Requests",
                "/requests");
        }

        if (ContainsAny(text, "help", "support", "contact", "what can you do", "options"))
        {
            return new(
                "I can guide you through account security, attendance, profile updates, access requests, account activity, and role-based administration. The Help & Support page also lists the portal's support paths.",
                "Open Help & Support",
                "/support");
        }

        return new(
            "I can help with account security, attendance, profile updates, access requests, account activity, and administrator access. Try asking, “How do I request access?” or choose one of the quick questions below.");
    }

    private static string FormatMinutes(int minutes)
    {
        var bounded = Math.Max(0, minutes);
        return $"{bounded / 60:00}:{bounded % 60:00}";
    }

    private static bool IsGreeting(string text) =>
        text is "hi" or "hello" or "hey"
        || text.StartsWith("hi ", StringComparison.Ordinal)
        || text.StartsWith("hello ", StringComparison.Ordinal)
        || text.StartsWith("hey ", StringComparison.Ordinal)
        || text.StartsWith("good morning", StringComparison.Ordinal)
        || text.StartsWith("good afternoon", StringComparison.Ordinal)
        || text.StartsWith("good evening", StringComparison.Ordinal);

    private static bool ContainsAny(string text, params string[] terms) =>
        terms.Any(text.Contains);
}

public sealed record SecureAssistantReply(
    string Message,
    string? ActionLabel = null,
    string? ActionHref = null);
