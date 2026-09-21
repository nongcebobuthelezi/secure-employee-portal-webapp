# Security Decisions

This document records the security choices demonstrated by the portfolio project. It is not a claim that a portfolio application has completed every control required for production certification.

## Identity and passwords

- ASP.NET Core Identity creates and verifies password hashes.
- Plain-text passwords are not persisted by application code.
- Public registration receives the least-privileged `Employee` role.
- Password requirements and failed-login lockout are configured centrally.
- Password changes update the Identity security stamp and authenticated session appropriately.

## Authentication cookies

- The authentication cookie is HTTP-only.
- Hosted environments require secure cookies.
- The cookie uses `SameSite=Lax`.
- Logout is a POST operation and validates an antiforgery token.

## Account status

`Active`, `Suspended` and `Disabled` are separate from Identity roles. A valid password is therefore not sufficient when the business account is blocked.

Long-lived Blazor sessions are periodically revalidated against the user record, account status and security stamp. Sensitive write operations also perform a fresh database-backed account/role check so a recently suspended or demoted session cannot keep changing protected data while waiting for the periodic revalidation interval.

## Authorization

- Employee pages require an authenticated user.
- Manager pages require `Manager` or `Administrator`.
- Security-administration pages require `Administrator`.
- Administrator role assignment is restricted to the documented `Employee`, `Manager` and `Administrator` allow-list even if an unrelated Identity role exists in the database.
- The current administrator cannot suspend, disable or re-role their own account through the UI, reducing accidental lockout.
- Administrator account-status and role changes invalidate the target account's security stamp before the protected change is applied; the change is aborted if existing sessions cannot be invalidated safely.
- Access requests do not grant permissions automatically; they create a reviewable record.
- An administrator cannot approve or reject an access request submitted by their own account.

## Password reset

Forgot-password responses remain neutral so callers cannot use the workflow to enumerate registered email addresses.

Development reset messages are written to `App_Data/development-mail/`, which is excluded from Git. Hosted environments require configured SMTP values. Reset tokens are not written to application logs. After a successful reset, the consumed token/email query string is replaced with a token-free confirmation URL and the component clears its submitted password values.

## Secure Assistant privacy boundary

The in-dashboard Secure Assistant is implemented locally in C# and does not send employee questions or account context to an external AI provider. It receives only the current portal role, account status and limited attendance context needed to choose bounded guidance and route suggestions. It is a support-navigation feature, not an authorization boundary and not a source of permission changes.

## Auditability

Security events record sign-in/account activity. Audit logs record important account and access-administration changes. Passwords, reset tokens and application secrets must never be written into either record type. Activity values are bounded to the database field limits, and an activity-recording failure is logged operationally rather than crashing a business action that has already completed.

## Configuration and deployment

- Production credentials are expected through environment variables, user secrets or a managed secret store.
- LocalDB is development-only; the application rejects a LocalDB connection string outside Development.
- Automatic migration execution is enabled for local Development and disabled by default in Production.
- Defensive response headers include `X-Content-Type-Options`, `X-Frame-Options` and `Referrer-Policy`.
- HSTS is enabled outside Development.

## Deliberate limitations

This project does not currently claim MFA, external identity federation, hardware-backed credentials, enterprise session management, penetration testing or compliance certification. Those are appropriate future extensions only if a real product requirement justifies them.
