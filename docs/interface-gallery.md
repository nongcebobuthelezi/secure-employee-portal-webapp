# Complete Interface Gallery

These screenshots come from the **SQL-backed runtime verification run**, not from static mockups. The workflow launched the .NET 10 application against an isolated SQL Server instance, exercised the real browser flows with Playwright, and captured normal viewport screenshots after each page passed the runtime health and horizontal-overflow checks.

> Runtime gallery: 26 screenshots · 0 document-level horizontal-overflow failures.

## Public authentication

| Sign in | Create account |
| --- | --- |
| <img src="images/runtime/01-login-desktop.png" alt="Secure Employee Portal sign-in screen" width="100%"> | <img src="images/runtime/02-register-desktop.png" alt="Secure Employee Portal registration screen" width="100%"> |

| Forgot password |
| --- |
| <img src="images/runtime/03-forgot-password-desktop.png" alt="Forgot password screen with neutral privacy message" width="100%"> |

## Employee experience

| Employee dashboard | Secure Assistant |
| --- | --- |
| <img src="images/runtime/04-employee-dashboard-desktop.png" alt="Authenticated employee dashboard" width="100%"> | <img src="images/runtime/26-secure-assistant-desktop.png" alt="Secure Assistant open on employee dashboard" width="100%"> |

| Profile | Security |
| --- | --- |
| <img src="images/runtime/05-employee-profile-desktop.png" alt="Employee profile page" width="100%"> | <img src="images/runtime/06-employee-security-desktop.png" alt="Employee security page" width="100%"> |

| Attendance | Access request |
| --- | --- |
| <img src="images/runtime/07-attendance-clocked-in-desktop.png" alt="Attendance page while employee is clocked in" width="100%"> | <img src="images/runtime/08-access-request-desktop.png" alt="Employee access request workflow" width="100%"> |

| Account activity | Help and support |
| --- | --- |
| <img src="images/runtime/09-account-activity-desktop.png" alt="Employee account activity timeline" width="100%"> | <img src="images/runtime/10-help-support-desktop.png" alt="Employee help and support page" width="100%"> |

## Authorization boundary

| Access denied |
| --- |
| <img src="images/runtime/11-access-denied-desktop.png" alt="Access denied page shown to an employee attempting an administrator route" width="100%"> |

## Administrator and manager experience

| Security administration | User management |
| --- | --- |
| <img src="images/runtime/12-admin-dashboard-desktop.png" alt="Administrator security dashboard" width="100%"> | <img src="images/runtime/13-user-management-desktop.png" alt="Administrator user management page" width="100%"> |

| Create employee | Employee account administration |
| --- | --- |
| <img src="images/runtime/14-create-employee-desktop.png" alt="Administrator create employee account page" width="100%"> | <img src="images/runtime/15-employee-account-desktop.png" alt="Administrator employee account controls" width="100%"> |

| Roles and permissions | Approved access request |
| --- | --- |
| <img src="images/runtime/16-roles-permissions-desktop.png" alt="Roles and permissions reference page" width="100%"> | <img src="images/runtime/17-access-review-approved-desktop.png" alt="Administrator-approved access request with reviewer and decision note" width="100%"> |

| Security events | Audit logs |
| --- | --- |
| <img src="images/runtime/18-security-events-desktop.png" alt="Administrator security events console" width="100%"> | <img src="images/runtime/19-audit-logs-desktop.png" alt="Administrator audit logs console" width="100%"> |

| Manager team view |
| --- |
| <img src="images/runtime/20-manager-team-desktop.png" alt="Limited manager team view" width="100%"> |

## Responsive views

These are normal **390 × 844** viewport captures rather than stitched full-page screenshots, so fixed/sticky UI is shown exactly as a user would encounter it.

| Mobile sign in | Mobile dashboard |
| --- | --- |
| <img src="images/runtime/21-login-mobile.png" alt="Mobile sign-in screen" width="55%"> | <img src="images/runtime/22-employee-dashboard-mobile.png" alt="Mobile employee dashboard" width="55%"> |

| Mobile profile | Mobile attendance |
| --- | --- |
| <img src="images/runtime/23-employee-profile-mobile.png" alt="Mobile employee profile" width="55%"> | <img src="images/runtime/24-attendance-mobile.png" alt="Mobile attendance page" width="55%"> |

| Mobile administrator dashboard |
| --- |
| <img src="images/runtime/25-admin-dashboard-mobile.png" alt="Mobile administrator dashboard" width="55%"> |

## What the gallery proves

The screenshots are evidence of the same runtime journey documented in [Runtime Verification](runtime-verification.md): public authentication, employee self-service, authorization denial, attendance persistence, access-request submission and review, administrator account management, role visibility, security events, audit logs, manager scope and responsive rendering.
